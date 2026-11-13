using Microsoft.AspNetCore.Diagnostics;
using Tally.Application;

namespace Tally.Api.Errors;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is InvalidCsvException)
        {
            logger.LogWarning("Rejected file: {Reason}", exception.Message);
            await Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid CSV", detail: exception.Message)
                .ExecuteAsync(httpContext);
            return true;
        }

        logger.LogError(exception, "Unexpected error on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        await Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Something went wrong")
            .ExecuteAsync(httpContext);
        return true;
    }
}
