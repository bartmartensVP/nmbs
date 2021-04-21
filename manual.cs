using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace vlapa.nmbs
{
    public static class manual
    {
        [Function("manual")]
        public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            tester t = new tester() ;
            BaseData bd = new BaseData() ;

            var logger = executionContext.GetLogger("manual");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            var test = System.Environment.GetEnvironmentVariable ("BMR") ;
            response.WriteString("Welcome to Azure Functions!" + t.bart + " stops known : "  + bd.stops.Count + " trips known : " + bd.trips.Count);
            return response;
        }
    }
}
