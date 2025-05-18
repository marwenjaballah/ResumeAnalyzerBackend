namespace ResumeAnalyzer.Models;

public class ResumeAnalysisResult
{
	public double Score { get; set; }
	public string ExtractedText { get; set; } = string.Empty;
	public string AiFeedback { get; set; } = string.Empty;
}
