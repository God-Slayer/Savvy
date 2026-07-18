using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Savvy.Api;

public sealed class OperationalMiddleware(
    RequestDelegate next,
    ILogger<OperationalMiddleware> logger
)
{
    public async Task Invoke(HttpContext context)
    {
        var correlation =
            context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Activity.Current?.Id
            ?? Guid.NewGuid().ToString("N");
        context.Response.Headers["X-Correlation-ID"] = correlation;
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception {CorrelationId}", correlation);
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Status = 500,
                        Title = "An unexpected error occurred.",
                        Detail = "Use the correlation ID when contacting support.",
                        Extensions = { ["correlationId"] = correlation },
                    }
                );
            }
        }
    }
}
