using System.Security.Cryptography;
using System.Text;

namespace Tally.Api.Middleware;

public sealed class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
{
    private const string HeaderName = "X-API-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        // Only the API needs a key; /health and Swagger stay open.
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        var providedKey = context.Request.Headers[HeaderName].ToString();
        if (!IsValid(providedKey))
        {
            logger.LogWarning("Rejected {Method} {Path}: missing or invalid API key", context.Request.Method, context.Request.Path);
            await Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Missing or invalid API key")
                .ExecuteAsync(context);
            return;
        }

        await next(context);
    }

    private bool IsValid(string providedKey)
    {
        var expectedKey = configuration["ApiKey"] ?? string.Empty;

        // FixedTimeEquals so the response time doesn't hint at how much of the key matched.
        return providedKey.Length > 0
            && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(providedKey), Encoding.UTF8.GetBytes(expectedKey));
    }
}
