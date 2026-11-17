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
            _logger.LogError(ex, "Exception handled by middleware: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            ArgumentException => (HttpStatusCode.BadRequest, "Validation Error"),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Operation Not Allowed"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource Not Found"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected server error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail = exception.Message,
            instance = context.Request.Path.Value
        };

        var responseJson = JsonSerializer.Serialize(problemDetails);
        return context.Response.WriteAsync(responseJson);
    }
}
