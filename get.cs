using System.Collections.Generic;
using System.Net;
using System.IO ;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace vlapa.nmbs
{
    public static class get
    {
        [Function("get")]
        public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            string dataPath = System.Environment.GetEnvironmentVariable("DATA_PATH");
            var logger = executionContext.GetLogger("get");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            //response.Headers.Add("Content-Type", "application/json");

            //response.WriteString("Welcome to Azure Functions!");

            var r = File.ReadAllText ( dataPath + "8833308.json") ;
            response.WriteAsJsonAsync<string>(r) ;

            return response;
        }
    }
}
