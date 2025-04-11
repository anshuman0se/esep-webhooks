using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;


[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogInformation($"Received input: {input}");

        try
        {

            dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());
            string issueUrl = json.issue.html_url;
            context.Logger.LogInformation($"Issue URL: {issueUrl}");
            string slackMessage = JsonConvert.SerializeObject(new
            {
                text = $"📢 New GitHub Issue Created: {issueUrl}"
            });
            string slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");
            if (string.IsNullOrEmpty(slackUrl))
            {
                return "SLACK_URL environment variable is missing.";
            }
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, slackUrl)
            {
                Content = new StringContent(slackMessage, Encoding.UTF8, "application/json")
            };
            var response = client.Send(request);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            string slackResponse = reader.ReadToEnd();
            return $"Slack response: {slackResponse}";
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Exception: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
}
