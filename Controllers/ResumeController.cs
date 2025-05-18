using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.Services;

namespace ResumeAnalyzer.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowNextJsApp")]
public class ResumeController : ControllerBase
{
    private readonly ResumeParser _parser;
    private readonly ResumeScorer _scorer;
    private readonly DeepSeekService _deepSeek;
    private readonly GeminiService _gemini;
    private readonly ILogger<ResumeController> _logger;

    public ResumeController(
        ResumeParser parser, 
        ResumeScorer scorer, 
        DeepSeekService deepSeek, 
        GeminiService gemini,
        ILogger<ResumeController> logger)
    {
        _parser = parser;
        _scorer = scorer;
        _deepSeek = deepSeek;
        _gemini = gemini;
        _logger = logger;
    }

    [HttpPost("analyze-gemini")]
    public async Task<IActionResult> AnalyzeWithGemini(IFormFile resume, [FromForm] string jobTitle)
    {
        try
        {
            _logger.LogInformation($"Received request - File: {resume?.FileName}, Job Title: {jobTitle}");

            if (resume == null || string.IsNullOrWhiteSpace(jobTitle))
                return BadRequest(new { message = "Resume and job title are required." });

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(uploads);
            var path = Path.Combine(uploads, resume.FileName);

            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await resume.CopyToAsync(stream);
            }

            var resumeText = _parser.ExtractText(path);
            var result = await _gemini.AnalyzeWithGemini(resumeText, jobTitle);

            if (result.Error != null)
            {
                _logger.LogWarning($"Error in Gemini analysis: {result.Error}");
                return Ok( result.Error);
            }

            _logger.LogInformation("Analysis completed successfully");
            return Ok(result.Feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing resume analysis");
            return StatusCode(500, new { message = "An error occurred while analyzing the resume" });
        }
        finally
        {
            // Clean up the uploaded file
            try
            {
                if (resume != null)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", resume.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up uploaded file");
            }
        }
    }

    [HttpPost("analyze-deepseek")]
    public async Task<IActionResult> Analyze(IFormFile resume, [FromForm] string jobTitle)
    {
        try
        {
            _logger.LogInformation($"Received request - File: {resume?.FileName}, Job Title: {jobTitle}");

            if (resume == null || string.IsNullOrEmpty(jobTitle))
                return BadRequest(new { message = "Resume and job title are required." });

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(uploads);

            var filePath = Path.Combine(uploads, resume.FileName);
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await resume.CopyToAsync(stream);
            }

            var resumeText = _parser.ExtractText(filePath);
            var score = _scorer.Score(resumeText, jobTitle);
            var aiResponse = await _deepSeek.AnalyzeWithAI(resumeText, jobTitle);

            var result = new ResumeAnalysisResult
            {
                Score = score,
                ExtractedText = resumeText,
                AiFeedback = aiResponse
            };

            _logger.LogInformation("Analysis completed successfully");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing resume analysis");
            return StatusCode(500, new { message = "An error occurred while analyzing the resume" });
        }
        finally
        {
            // Clean up the uploaded file
            try
            {
                if (resume != null)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", resume.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up uploaded file");
            }
        }
    }
}