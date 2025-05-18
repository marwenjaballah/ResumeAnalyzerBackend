using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ResumeAnalyzer.Services;

public class StructuredFeedback
{
    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("missingSkills")]
    public List<string> MissingSkills { get; set; } = new();

    [JsonPropertyName("suggestions")]
    public List<string> Suggestions { get; set; } = new();

    [JsonPropertyName("analysis")]
    public string Analysis { get; set; } = string.Empty;

    [JsonPropertyName("keywordMatch")]
    public Dictionary<string, int> KeywordMatch { get; set; } = new();

    [JsonPropertyName("missingKeywords")]
    public List<string> MissingKeywords { get; set; } = new();

    [JsonPropertyName("rewrites")]
    public List<RewriteSuggestion> Rewrites { get; set; } = new();
    

    [JsonPropertyName("sectionScores")]
    public Dictionary<string, int> SectionScores { get; set; } = new();


}

public class RewriteSuggestion
{
    [JsonPropertyName("original")]
    public string Original { get; set; } = string.Empty;

    [JsonPropertyName("improved")]
    public string Improved { get; set; } = string.Empty;
}


public class StructuredFeedbackError
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
}
public class StructuredFeedbackResult
{
    public StructuredFeedback? Feedback { get; set; }
    public StructuredFeedbackError? Error { get; set; }
}
public class GeminiService
{
    private readonly HttpClient _client;
    private readonly string _apiKey;

    public GeminiService(IConfiguration config)
    {
        _apiKey = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key is not configured.");
        _client = new HttpClient();
    }

    public async Task<StructuredFeedbackResult> AnalyzeWithGemini(string resume, string jobDesc)
{
    var prompt = 
$"""
                      You are a professional resume analyzer capable of assessing candidates across all industries and job types.

                      Your task is to analyze the following resume against the provided job description.

                      Before generating the analysis:
                      - Check if the resume contains relevant professional or educational content.
                      - Check if the job description is existent as a job not just a random characters.
                      

                      If either the resume or job description is missing or invalid, return a JSON response with one of the following messages:
                      
                          "error": "Resume is too short or not provided"
                      
                          "error": "Job description is inexistent or invalid"
                      
                         


                      Only if both inputs are valid, return a JSON object with the following structure:

                      - "score": number (0-100) — overall fit score
                      - "missingSkills": array of strings — key skills required by the job but not found in the resume
                      - "suggestions": array of strings — actionable improvements, each with a concrete example or rewrite
                      - "analysis": string — summary of strengths and areas for improvement
                      - "keywordMatch": object — percentage match by domain-relevant categories (e.g., customer service, technical, leadership, compliance, marketing, creative, etc.)
                      - "missingKeywords": array of strings — specific industry or role-related terms missing from the resume
                      - "rewrites": array of objects with "original" and "improved"
                      - "sectionScores": object — scores (0-100) for each section: summary, experience, education, and skills

                      Resume:
                      ----
                      {resume}
                      ----

                      Job Description:
                      ----
                      {jobDesc}
                      ----
                      """;

    var requestBody = new
    {
        contents = new[]
        {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
    };

    var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
    var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

    try
    {
        var response = await _client.PostAsync(url, jsonContent);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new StructuredFeedbackResult
            {
                Error = new StructuredFeedbackError
                {
                    Error = $"Gemini API error {response.StatusCode}: {responseText}"
                }
            };
        }

        using var doc = JsonDocument.Parse(responseText);
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
        {
            return new StructuredFeedbackResult
            {
                Error = new StructuredFeedbackError
                {
                    Error = "Gemini returned empty or invalid content."
                }
            };
        }

        var cleanJson = StripCodeFences(text!);

        using var jsonDoc = JsonDocument.Parse(cleanJson);
        if (jsonDoc.RootElement.TryGetProperty("error", out var errorProp))
        {
            return new StructuredFeedbackResult
            {
                Error = new StructuredFeedbackError
                {
                    Error = errorProp.GetString() ?? "Unknown error from Gemini."
                }
            };
        }

        var feedback = JsonSerializer.Deserialize<StructuredFeedback>(cleanJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (feedback == null)
        {
            return new StructuredFeedbackResult
            {
                Error = new StructuredFeedbackError
                {
                    Error = "Gemini returned invalid JSON."
                }
            };
        }

        return new StructuredFeedbackResult { Feedback = feedback };
    }
    catch (Exception ex)
    {
        return new StructuredFeedbackResult
        {
            Error = new StructuredFeedbackError
            {
                Error = ex.Message
            }
        };
    }
}


    private static string StripCodeFences(string raw)
    {
        return raw
            .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
            .Replace("```", "")
            .Trim();
    }
}

