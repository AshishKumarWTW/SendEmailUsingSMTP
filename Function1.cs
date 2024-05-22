using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace SendEmailUsingSMTP
{
    public static class Function1
    {
        [Function(nameof(Function1))]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(Function1));
            logger.LogInformation("Start Email Operation");
            //var outputs = new List<string>();

            // Replace name and input with values relevant for your Durable Functions Activity
            var emailfrom = "ashish70kumar@gmail.com";
            await context.CallActivityAsync<string>(nameof(SendSuccessEmail), emailfrom);

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return "EMail Operation Complete";   
        }


        [Function(nameof(SendSuccessEmail))]
        public static void SendSuccessEmail([ActivityTrigger] string emailfrom, ILogger log)
        {
            
            var client = new SmtpClient("smtp-n20.cet.willistowerswatson.com", 25);
            var FName = "DemoFile.txt";
            var rCount = 2192;
            var isSent = "Yes";

            client.SendMailAsync(new MailMessage(from: emailfrom
                                , to: "ashish.kumar2@wtwco.com",// to email address
                                subject: "Test Email : You have recieved a test Mail", //email subject
                                body: "\r FileName : " + FName + "\r\n Row Count : " + rCount + "\r\n Sent/Non Empty? : " + isSent//Mail body
                            ));

            

        }
        [Function("Function1_HttpStart")]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("Function1_HttpStart");

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(Function1));

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return await client.CreateCheckStatusResponseAsync(req, instanceId);
        }
    }
}
