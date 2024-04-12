using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace functions_azure_batch_if
{
    public class BatchStart
    {
        private readonly ILogger _logger;

        static BatchStart()
        {
            //var root = Directory.GetCurrentDirectory();
            //var dotenv = Path.Combine(root, ".env");
            //DotNetEnv.Env.Load(dotenv);
            DotNetEnv.Env.Load();
        }
        public BatchStart(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BatchStart>();
        }

        [Function("BatchStart")]
        async public Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            //AzureBatchRequest(string poolId, string jobId, string taskId, string commandLine, string batchAccountUrl, string batchAccountName, string batchAccountKey, TimeSpan timeout)
            var timeStamp = DateTime.Now.ToString("yyyyMMddhhmmssSSS");
            var ba = new AzureBatch(
                new AzureBatchRequest(
                    poolId: DotNetEnv.Env.GetString("BATCH_POOL_ID"),
                    jobId: $"{DotNetEnv.Env.GetString("BATCH_JOB_ID")}{timeStamp}",
                    taskId: $"taskid{timeStamp}",
                    commandLine: $"powershell -Command \"echo 'Hello World! {timeStamp}'\"",
                    batchAccountUrl: DotNetEnv.Env.GetString("BATCH_ACCOUNT_URL"),
                    batchAccountName: DotNetEnv.Env.GetString("BATCH_ACCOUNT_NAME"),
                    batchAccountKey: DotNetEnv.Env.GetString("BATCH_ACCOUNT_KEY"),
                    timeout: TimeSpan.FromMinutes(30)
                ),
                logger: this._logger);
            var r = ba.doSubmit();
            var jsonToReturn = JsonConvert.SerializeObject(r);
            await response.WriteStringAsync(jsonToReturn);
            return response;
        }
    }
}
