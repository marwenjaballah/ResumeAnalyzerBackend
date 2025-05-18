using System.Text.RegularExpressions;

namespace ResumeAnalyzer.Services;

public class ResumeScorer
{
	private readonly HashSet<string> _stopwords;

	public ResumeScorer()
	{
		var stopwordFile = Path.Combine(Directory.GetCurrentDirectory(), "stopwords.txt");
		_stopwords = File.Exists(stopwordFile)
			? new HashSet<string>(File.ReadAllLines(stopwordFile))
			: new HashSet<string>();
	}

	public double Score(string resume, string job)
	{
		var resumeWords = CleanAndTokenize(resume);
		var jobWords = CleanAndTokenize(job);

		var common = jobWords.Intersect(resumeWords).Count();
		return jobWords.Count == 0 ? 0 : (double)common / jobWords.Count * 100;
	}

	private List<string> CleanAndTokenize(string text)
	{
		return Regex.Split(text.ToLower(), @"\W+")
			.Where(w => !_stopwords.Contains(w) && w.Length > 2)
			.ToList();
	}
}
