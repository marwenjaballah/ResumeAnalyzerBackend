using UglyToad.PdfPig;

namespace ResumeAnalyzer.Services;

public class ResumeParser
{
	public string ExtractText(string path)
	{
		using var document = PdfDocument.Open(path);
		var text = string.Join(Environment.NewLine, document.GetPages().Select(p => p.Text));
		return text;
	}
}
