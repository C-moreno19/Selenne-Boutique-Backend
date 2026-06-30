using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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

        HttpStatusCode status;
        string message;

        if (ex is UnauthorizedException)
        {
            status = HttpStatusCode.Unauthorized;
            message = ex.Message;
        }
        else if (ex is ForbiddenException)
        {
            status = HttpStatusCode.Forbidden;
            message = ex.Message;
        }
        else if (ex is AppException appEx)
        {
            status = (HttpStatusCode)appEx.StatusCode;
            message = appEx.Message;
        }
        else if (ex is DbUpdateException dbEx)
        {
            status = HttpStatusCode.BadRequest;
            var inner = dbEx.InnerException?.Message ?? dbEx.Message;
            if (inner.Contains("REFERENCE") || inner.Contains("FK_") || inner.Contains("FOREIGN KEY"))
                message = "No se puede realizar esta operación porque el registro está siendo utilizado en otras partes del sistema (pedidos, ventas, compras u otros módulos).";
            else
                message = $"Error al guardar en la base de datos: {inner}";
        }
        else
        {
            status = HttpStatusCode.InternalServerError;
            message = "Error interno del servidor";
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        var response = JsonSerializer.Serialize(new { success = false, message, errors = (object?)null });
        await context.Response.WriteAsync(response);
    }
}
