using System.Net;
using System.Text.Json;

namespace TestPilot.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "An unexpected error occurred processing your request.",
            status = context.Response.StatusCode,
            detail = exception.Message,
            instance = context.Request.Path.Value
        };

        var responseJson = JsonSerializer.Serialize(problemDetails);
        return context.Response.WriteAsync(responseJson);
    }
}
