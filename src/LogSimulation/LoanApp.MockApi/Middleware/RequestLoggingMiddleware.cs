using System.Text.Json;

namespace LoanApp.MockApi.Middleware;

public class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _log;
    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> log) => _log = log;

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        var started = DateTime.UtcNow;
        var traceId = ctx.Request.Headers["traceparent"].FirstOrDefault() ?? $"00-{Guid.NewGuid():N}-01";
        ctx.Items["traceId"] = traceId;
        try
        {
            await next(ctx);
        }
        finally
        {
            var latency = (int)(DateTime.UtcNow - started).TotalMilliseconds;
            var record = new
            {
                ts = DateTime.UtcNow,
                level = "Information",
                msg = "http_request",
                traceId,
                http = new {
                    method = ctx.Request.Method,
                    route = ctx.Request.Path.Value,
                    status = ctx.Response.StatusCode,
                    latencyMs = latency
                }
            };
            _log.LogInformation(JsonSerializer.Serialize(record));
        }
    }
}