namespace SelenneApi.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Dictionary<string, (int count, DateTime reset)> _counts = new();

    public RateLimitingMiddleware(RequestDelegate next) { _next = next; }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        lock (_counts)
        {
            if (_counts.TryGetValue(key, out var entry))
            {
                if (DateTime.Now > entry.reset) _counts[key] = (1, DateTime.Now.AddMinutes(1));
                else if (entry.count > 100) { context.Response.StatusCode = 429; return; }
                else _counts[key] = (entry.count + 1, entry.reset);
            }
            else _counts[key] = (1, DateTime.Now.AddMinutes(1));
        }
        await _next(context);
    }
}
