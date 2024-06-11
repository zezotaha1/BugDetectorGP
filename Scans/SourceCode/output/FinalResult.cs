using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace GBugDetectorGP.Scans.SourceCode.output
{
    public class FinalResult
    {
        private static readonly string apiKey = "AIzaSyCGbBqIOCqVV24-PeAVvRsERxZwlQDhklo";
        private readonly string url;
        private readonly string InjectedFunction;
        public string SolutionScanResults  = "";

        public FinalResult(string injectedFunction)
        {
            InjectedFunction = injectedFunction;
            url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}";
            GetFinalResultAsync().Wait();
        }

        private async Task GetFinalResultAsync()
        {
            string x = $"I have a SQL function vulnerable to SQL injection. Here is the function:\n\n{InjectedFunction}\n\n" +
                       "Please provide a mitigation function to fix the SQL injection vulnerability and explain what happened and how the mitigation works.";

            var jsonPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = x }
                        }
                    }
                }
            };

            string contentJson = JsonSerializer.Serialize(jsonPayload);
            var content = new StringContent(contentJson, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                JsonNode jsonResponse = JsonNode.Parse(responseString);

                string generatedContent = jsonResponse?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString() ?? "";

                // Split the generated content based on sections
                string[] splitContent = generatedContent.Split(new string[] { "```", "*Improvements:", "Additional Considerations:*" }, StringSplitOptions.None);

                // Mitigation function is inside the first code block
                string mitigationFunction = splitContent.Length > 1 ? splitContent[1] : "No mitigation function found.";

                // Explanation is after the mitigation function
                string explanation = splitContent.Length > 2 ? splitContent[2] : "No explanation found.";

                SolutionScanResults+="##########"+InjectedFunction.Replace("#","*");
                SolutionScanResults+= "##########" + mitigationFunction.Replace("#", "*");
                SolutionScanResults += "##########" + explanation.Replace("#", "*");
            }
            catch (HttpRequestException e)
            {
                SolutionScanResults = $"Request error: {e.Message}";
            }
            catch (JsonException e)
            {
                SolutionScanResults=$"JSON error: {e.Message}";
            }
            catch (Exception e)
            {
                SolutionScanResults=$"Unexpected error: {e.Message}";
            }
        }

        public string GetResults()
        {
            return SolutionScanResults;
        }
    }
}
