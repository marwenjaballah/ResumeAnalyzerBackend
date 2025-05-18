using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ResumeAnalyzer.Services;

public class DeepSeekService
{
    private readonly HttpClient _client;
    private readonly string _apiKey;

    public DeepSeekService(IConfiguration config)
    {
        _apiKey = config["DeepSeek:ApiKey"] ?? throw new InvalidOperationException("DeepSeek API key is not configured.");
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> AnalyzeWithAI(string resume, string jobDesc)
    {
        var prompt = $"""
            You are a resume screening assistant.
            Given this resume:
            ----
            {resume}
            ----
            And this job description:
            ----
            {jobDesc}
            ----
            Provide:
            - A match score from 0 to 100
            - A list of missing skills
            - Suggestions to improve the resume
            """;

        var payload = new
        {
            model = "deepseek-chat", // or "deepseek-coder"
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("https://api.deepseek.com/v1/chat/completions", content);
        var json = await response.Content.ReadAsStringAsync();

        try
        {
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("choices", out var choices) ||
                choices.GetArrayLength() == 0)
            {
                return $"❌ DeepSeek error: Unexpected response format.\n\n{json}";
            }

            var message = choices[0].GetProperty("message").GetProperty("content").ToString();
            return message;
        }
        catch (Exception ex)
        {
            return $"❌ Failed to parse DeepSeek response: {ex.Message}\n\nRaw response:\n{json}";
        }
    }
}
