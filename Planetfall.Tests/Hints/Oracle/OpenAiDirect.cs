using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Planetfall.Tests.Hints.Oracle;

/// <summary>A direct chat-completions call with a long timeout and retries, for offline tooling (big reads, long writes, grading).</summary>
internal static class OpenAiDirect
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(20) };

    public static async Task<string> Ask(string model, string system, string user)
    {
        var key = Environment.GetEnvironmentVariable("OPEN_AI_KEY") ?? throw new InvalidOperationException("OPEN_AI_KEY is not set.");
        var body = JsonConvert.SerializeObject(new
        {
            model,
            messages = new object[] { new { role = "system", content = system }, new { role = "user", content = user } }
        });

        for (var attempt = 1;; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            try
            {
                using var response = await Http.SendAsync(request);
                var text = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                    return JObject.Parse(text)["choices"]![0]!["message"]!["content"]!.ToString();
                if (attempt >= 4 || (int)response.StatusCode is not (429 or >= 500))
                    throw new InvalidOperationException($"OpenAI {(int)response.StatusCode}: {text[..Math.Min(text.Length, 600)]}");
            }
            catch (HttpRequestException) when (attempt < 4)
            {
            }

            await Task.Delay(TimeSpan.FromSeconds(20 * attempt));
        }
    }
}
