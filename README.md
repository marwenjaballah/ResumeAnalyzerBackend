# Resume Analyzer Backend

A C# ASP.NET Core Web API for analyzing resumes against job descriptions using advanced AI (Gemini & DeepSeek) and providing structured feedback for candidates.

## Features

- REST API for submitting resumes and job descriptions
- Integrates with Google Gemini and DeepSeek AI for evaluation
- Returns actionable feedback, missing skills, and scoring
- Ready for deployment to Render, Railway, or any Docker host
- CORS configured for secure frontend/backend integration

## API Endpoints

**POST `/api/resume/analyze-gemini`**  
Analyze a resume against a job description using Gemini AI.

**POST `/api/resume/analyze-deepseek`**  
Analyze a resume using DeepSeek AI.

**GET `/swagger`**  
OpenAPI docs for exploring/testing the API.

## Example Request

```
POST /api/resume/analyze-gemini
Content-Type: application/json

{
  "resume": "Your resume text or content here",
  "jobDescription": "Job description text here"
}
```

## Example Response

```
{
  "score": 78,
  "missingSkills": ["Docker", "Kubernetes", "GraphQL"],
  "suggestions": [
    "Add specific metrics to demonstrate the impact of your projects",
    "Include more details about your experience with cloud platforms",
    "Highlight team collaboration and leadership skills"
  ],
  "analysis": "The resume shows strong technical skills in software development with experience in multiple programming languages. Consider adding more quantifiable achievements and specific project outcomes.",
  "keywordMatch": {
    "frontend": 90,
    "backend": 75,
    "cloud": 60
  },
  "missingKeywords": [
    "CI/CD",
    "Microservices",
    "Test-Driven Development",
    "Agile Methodologies"
  ],
  "rewrites": [
    {
      "original": "Developed a web application for customer management",
      "improved": "Developed and deployed a responsive web application that increased customer engagement by 35% and streamlined management processes, reducing administrative time by 20%"
    },
    {
      "original": "Implemented database optimization techniques",
      "improved": "Implemented advanced database optimization techniques that reduced query response times by 60% and decreased server load by 45% during peak usage periods"
    }
  ],
  "sectionScores": {
    "summary": 80,
    "experience": 85,
    "education": 70,
    "skills": 75
  }
}
```

## Error Responses

### Invalid Job Description
```
{
  "error": "Job description is inexistent or invalid"
}
```

### Invalid Resume
```
{
  "error": "Resume is too short or not provided"
}
```

## Local Development

### Prerequisites

- .NET 8 SDK
- Git
- (Optional) Docker

### Steps

1. Clone the repository:
```
git clone https://github.com/YOUR_USERNAME/ResumeAnalyzerBackend.git
cd ResumeAnalyzerBackend
```

2. Set your API keys:
   - Gemini:ApiKey in environment variables or appsettings.Development.json

3. Run the application:
```
dotnet run
```

4. Open your browser at http://localhost:5207/swagger

## Deployment

- **Render**: Connect your repo, use a Dockerfile or .NET 8 Web Service.
- **Railway**: Connect GitHub, set build/start commands.

## Configuration

### CORS

CORS is enabled for:
- http://localhost:3000 (local Next.js)
- https://ai-resume-analyzer-beryl.vercel.app (your production frontend)

Change these in Program.cs if your frontend domain changes.

### Environment Variables

- `Gemini:ApiKey` — Google Gemini API key
- (Add more as needed)

## Project Structure

```
ResumeAnalyzerBackend/
├── Controllers/
│   └── ResumeController.cs
├── Services/
│   ├── IGeminiService.cs
│   ├── GeminiService.cs
│   ├── IDeepSeekService.cs
│   └── DeepSeekService.cs
├── Models/
│   ├── ResumeAnalysisRequest.cs
│   └── ResumeAnalysisResponse.cs
├── Program.cs
├── Dockerfile
├── README.md
└── ...
```

## Contributing

Pull requests are welcome! For major changes, open an issue first to discuss what you want to change.

## License

MIT

## Author

Marwen Jaballah
