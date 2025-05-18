using ResumeAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add CORS service
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowNextJsApp", builder =>
	{
		builder.WithOrigins(
				"http://localhost:3000", // <--- NO TRAILING SLASH!
				"https://ai-resume-analyzer-beryl.vercel.app"
			)
			.AllowAnyMethod()
			.AllowAnyHeader();
	});
});

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ResumeParser>();
builder.Services.AddSingleton<ResumeScorer>();
builder.Services.AddSingleton<DeepSeekService>();
builder.Services.AddSingleton<GeminiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Add CORS middleware BEFORE other middleware
app.UseCors("AllowNextJsApp");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
