using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
            if (dbEx.InnerException is PostgresException pgEx)
            {
                message = pgEx.SqlState switch
                {
                    PostgresErrorCodes.UniqueViolation => MensajeDeDuplicado(pgEx.ConstraintName),
                    PostgresErrorCodes.ForeignKeyViolation => "No se puede realizar esta operación porque el registro está siendo utilizado en otras partes del sistema (pedidos, ventas, compras u otros módulos).",
                    _ => $"[DEBUG-TEMP] {pgEx.SqlState} | {pgEx.MessageText} | constraint={pgEx.ConstraintName} | column={pgEx.ColumnName} | table={pgEx.TableName}",
                };
            }
            else
            {
                var inner = dbEx.InnerException?.Message ?? dbEx.Message;
                message = inner.Contains("REFERENCE") || inner.Contains("FK_") || inner.Contains("FOREIGN KEY")
                    ? "No se puede realizar esta operación porque el registro está siendo utilizado en otras partes del sistema (pedidos, ventas, compras u otros módulos)."
                    : "Error al guardar en la base de datos.";
            }
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

    // El nombre de la constraint (ej. "Usuarios_Email_Key") es lo unico que
    // Postgres nos da sin exponer el detalle crudo de la fila duplicada.
    private static string MensajeDeDuplicado(string? constraintName)
    {
        if (constraintName != null && constraintName.Contains("Email", StringComparison.OrdinalIgnoreCase))
            return "Ese correo electrónico ya está registrado.";
        return "Ya existe un registro con esos datos.";
    }
}
