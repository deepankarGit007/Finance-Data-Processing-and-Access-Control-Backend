using System.Net;
using System.Text.Json;

namespace FinanceBackend.Middleware;

/// <summary>
/// Catches all unhandled exceptions and maps them to structured JSON error responses.
/// This keeps controller code clean — services just throw standard exceptions.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        var (statusCode, code) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,  "UNAUTHORIZED"),
            KeyNotFoundException        => (HttpStatusCode.NotFound,       "NOT_FOUND"),
            InvalidOperationException   => (HttpStatusCode.Conflict,       "CONFLICT"),
            ArgumentException           => (HttpStatusCode.BadRequest,     "BAD_REQUEST"),
            _                           => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR")
        };

        var message = statusCode == HttpStatusCode.InternalServerError
            ? "An unexpected error occurred. Please try again later."
            : ex.Message;

        var body = JsonSerializer.Serialize(new
        {
            code,
            detail = message
        });

        ctx.Response.StatusCode  = (int)statusCode;
        ctx.Response.ContentType = "application/json";

        return ctx.Response.WriteAsync(body);
    }
}
