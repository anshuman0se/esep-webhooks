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
        try
        {
            dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());
            string issueUrl = json.issue.html_url;
            string slackMessage = JsonConvert.SerializeObject(new { text = $"New GitHub Issue Created: {issueUrl}" });

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
            {
                Content = new StringContent(slackMessage, Encoding.UTF8, "application/json")
            };

            var response = client.Send(request);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
