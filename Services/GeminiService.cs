using System.Text.Json;
using System.Text;

namespace AIChatApp.Services
{
    public class GeminiService
    {
        private readonly IConfiguration _configuration;

        public GeminiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> AskAI(string prompt)
        {
            try
            {
                var apiKey = _configuration["Gemini:ApiKey"];

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

                using var client = new HttpClient();

                var requestBody = new
                {
                    contents = new[]
                    {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            }
                };

                var json = JsonSerializer.Serialize(requestBody);

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(url, content);

                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine(result);

                using JsonDocument doc = JsonDocument.Parse(result);

                if (doc.RootElement.TryGetProperty("candidates", out JsonElement candidates))
                {
                    var aiResponse =
                        candidates[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return aiResponse;
                }

                if (doc.RootElement.TryGetProperty("error", out JsonElement error))
                {
                    return $"AI service temporarily unavailable. Please try again later: {error}";
                }

                return "Unknown response from Gemini API";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
