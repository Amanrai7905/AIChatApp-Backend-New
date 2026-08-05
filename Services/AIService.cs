using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace AIChatApp.Services
{
    public class AIService
    {
        private readonly IConfiguration _configuration;

        public AIService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> AskAI(string prompt)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "gpt-4o-mini",
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
                "https://api.openai.com/v1/chat/completions",
                content);

            var result = await response.Content.ReadAsStringAsync();

            return result;
        }
    }
}
