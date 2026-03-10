using System.Net;
using System.Text.Json;
using SelenneApi.Exceptions;

namespace SelenneApi.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex) { await HandleExceptionAsync(context, ex); }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        var (status, message) = ex switch
        {
            UnauthorizedException => (HttpStatusCode.Unauthorized, ex.Message),
            ForbiddenException => (HttpStatusCode.Forbidden, ex.Message),
            AppException appEx => ((HttpStatusCode)appEx.StatusCode, appEx.Message),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor")
        };
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        var response = JsonSerializer.Serialize(new { success = false, message, errors = (object?)null });
        await context.Response.WriteAsync(response);
    }
}
