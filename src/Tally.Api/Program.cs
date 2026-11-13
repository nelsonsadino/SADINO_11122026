using Tally.Api.Middleware;
using Tally.Api.Swagger;
using Tally.Application;
using Tally.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<ApiKeySecuritySchemeTransformer>());

builder.Services.AddSingleton<IFileTracker, InMemoryFileTracker>();
builder.Services.AddScoped<FileService>();

var app = builder.Build();

if (string.IsNullOrWhiteSpace(app.Configuration["ApiKey"]))
{
    throw new InvalidOperationException("ApiKey is not set. Add it to appsettings or set the ApiKey environment variable.");
}

app.UseMiddleware<ApiKeyMiddleware>();

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Tally API"));

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
