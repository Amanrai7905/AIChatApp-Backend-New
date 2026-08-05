using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace AIChatApp.Services
{
    public class GroqService
    {
        private readonly IConfiguration _configuration;

        public GroqService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> AskAI(string prompt)
        {
            try
            {
                var apiKey = _configuration["Groq:ApiKey"];

                using var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        apiKey);

                var requestBody = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
     {
        new
        {
            role = "user",
            content = prompt
        }
    }
                };

                var json = JsonSerializer.Serialize(requestBody);

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    "https://api.groq.com/openai/v1/chat/completions",
                    content);

                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine(result);

                using JsonDocument doc = JsonDocument.Parse(result);

                // SUCCESS RESPONSE
                if (doc.RootElement.TryGetProperty("choices", out JsonElement choices))
                {
                    var aiResponse =
                       choices[0]
                       .GetProperty("message")
                       .GetProperty("content")
                       .GetString();
                    return aiResponse;

                    //return choices[0]
                    //    .GetProperty("message")
                    //    .GetProperty("content")
                    //    .GetString();
                }

                // ERROR RESPONSE
                if (doc.RootElement.TryGetProperty("error", out JsonElement error))
                {
                    return $"Groq Error: {error}";
                }

                return "Unknown response from Groq";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
