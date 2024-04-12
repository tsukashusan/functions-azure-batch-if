using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace functions_azure_batch_if
{
    public class BatchStart
    {
        private readonly ILogger _logger;

        public BatchStart(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BatchStart>();
        }

        [Function("BatchStart")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            //AzureBatchRequest(string poolId, string jobId, string taskId, string commandLine, string batchAccountUrl, string batchAccountName, string batchAccountKey, TimeSpan timeout)
            var timeStamp = DateTime.Now.ToString("yyyyMMddhhmmssSSS");
            var ba = new AzureBatch(
                new AzureBatchRequest(
                    poolId: DotNetEnv.Env.GetString("BATCH_POOL_ID"),
                    jobId: $"{DotNetEnv.Env.GetString("BATCH_JOB_ID")}_{timeStamp}",
                    taskId: $"taskid_{timeStamp}",
                    commandLine: "",
                    batchAccountUrl: DotNetEnv.Env.GetString("BATCH_JOB_ID"),
                    batchAccountName: DotNetEnv.Env.GetString("BATCH_JOB_ID"),
                    batchAccountKey: DotNetEnv.Env.GetString("BATCH_JOB_ID"),
                    timeout: TimeSpan.FromMinutes(30)
                ),
                logger: this._logger);
            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
