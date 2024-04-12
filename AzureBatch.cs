using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Extensions.Logging;


struct AzureBatchRequest
{

    public readonly string PoolId;
    public readonly string JobId;
    public readonly string TaskId;
    public readonly string CommandLine;
    public readonly string BatchAccountUrl;
    public readonly string BatchAccountName;
    public readonly string BatchAccountKey;
    public readonly TimeSpan Timeout;
    public AzureBatchRequest(string poolId, string jobId, string taskId, string commandLine, string batchAccountUrl, string batchAccountName, string batchAccountKey, TimeSpan timeout)
    {
        PoolId = poolId;
        JobId = jobId;
        TaskId = taskId;
        CommandLine = commandLine;
        BatchAccountUrl = batchAccountUrl;
        BatchAccountName = batchAccountName;
        BatchAccountKey = batchAccountKey;
        Timeout = timeout;
    }
}
struct AzureBatchResponse
{
    public readonly string TaskId;
    public readonly string NodeId;
    public readonly string Stdout;
    public readonly bool Result;
    public readonly Exception? Exception;
    public AzureBatchResponse(string taskId, string nodeId, string stdout, bool result, Exception? exception = null)
    {
        TaskId = taskId;
        NodeId = nodeId;
        Stdout = stdout;
        Result = result;
        Exception = exception;
    }
}
class AzureBatch
{

    readonly AzureBatchRequest AzureBatchRequest;

    readonly ILogger? ILogger;

    private AzureBatch()
    {
        ILogger = null;
    }

    public AzureBatch(AzureBatchRequest azureBatchRequest, ILogger logger)
    {
        this.AzureBatchRequest = azureBatchRequest;
        this.ILogger = logger;
    }

    public IList<AzureBatchResponse> doSubmit()
    {
        try
        {
            var cred = new BatchSharedKeyCredentials(this.AzureBatchRequest.BatchAccountUrl, this.AzureBatchRequest.BatchAccountName, this.AzureBatchRequest.BatchAccountKey);
            using BatchClient batchClient = BatchClient.Open(cred);
            var jobid = $"{this.AzureBatchRequest.JobId}";
            ILogger.LogInformation("Creating job [{0}]...", jobid);
            try
            {
                CloudJob job = batchClient.JobOperations.CreateJob();
                job.Id = jobid;
                job.PoolInformation = new PoolInformation { PoolId = this.AzureBatchRequest.PoolId };
                job.Commit();
            }
            catch (BatchException be)
            {
                // Accept the specific error code JobExists as that is expected if the job already exists
                if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.JobExists)
                {
                    ILogger.LogWarning("The job {0} already existed when we tried to create it", jobid);
                    ILogger.LogCritical(be.Message);
                    ILogger.LogCritical(be.StackTrace);
                }
                else
                {
                    ILogger.LogCritical(be.Message);
                    ILogger.LogCritical(be.StackTrace);
                    return new List<AzureBatchResponse>() { { new AzureBatchResponse(taskId: string.Empty, nodeId: string.Empty, stdout: string.Empty, result: false, exception: be) } };
                }
            }

            var tasks = new List<CloudTask>() { new CloudTask(this.AzureBatchRequest.TaskId, this.AzureBatchRequest.CommandLine) };

            // Add all tasks to the job.
            batchClient.JobOperations.AddTask(jobid, tasks);
            // Monitor task success/failure, specifying a maximum amount of time to wait for the tasks to complete.
            TimeSpan timeout = TimeSpan.FromMinutes(30);
            ILogger.LogInformation("Monitoring all tasks for 'Completed' state, timeout in {0}...", timeout);
            IEnumerable<CloudTask> addedTasks = batchClient.JobOperations.ListTasks(jobid);
            batchClient.Utilities.CreateTaskStateMonitor().WaitAll(addedTasks, TaskState.Completed, timeout);

            ILogger.LogInformation("All tasks reached state Completed.");
            // Print task output
            ILogger.LogInformation("Printing task output...");
            IEnumerable<CloudTask> completedtasks = batchClient.JobOperations.ListTasks(jobid);
            IList<AzureBatchResponse> l = new List<AzureBatchResponse>();
            foreach (CloudTask task in completedtasks)
            {
                var o = new AzureBatchResponse(taskId: task.Id, nodeId: string.Format(task.ComputeNodeInformation.ComputeNodeId), stdout: task.GetNodeFile(Constants.StandardOutFileName).ReadAsString(), result: true);
                l.Add(o);
                ILogger.LogInformation("Task: {0}", o.TaskId);
                ILogger.LogInformation("Node: {0}", o.NodeId);
                ILogger.LogInformation("Standard out:");
                ILogger.LogInformation(o.Stdout);
            }
            return l;
        }
        catch (Exception e)
        {
            ILogger.LogCritical(e.Message);
            ILogger.LogCritical(e.StackTrace);
            return new List<AzureBatchResponse>() { { new AzureBatchResponse(taskId: string.Empty, nodeId: string.Empty, stdout: string.Empty, result: false, exception: e) } };
        }
    }
}