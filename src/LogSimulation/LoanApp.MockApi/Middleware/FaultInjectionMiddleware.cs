using LoanApp.MockApi.Services;
using System.Text.Json;

namespace LoanApp.MockApi.Middleware;

public class FaultInjectionMiddleware : IMiddleware
{
    private readonly FaultRegistry _registry;
    private readonly ILogger<FaultInjectionMiddleware> _log;
    private readonly Random _rng = new();

    public FaultInjectionMiddleware(FaultRegistry registry, ILogger<FaultInjectionMiddleware> log)
    {
        _registry = registry; _log = log;
    }

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        var prof = _registry.Resolve(ctx);
        if (prof is not null)
        {
            if (prof.LatencyMs > 0) await Task.Delay(prof.LatencyMs);

            var injectErr = prof.AbortHttpStatus > 0 || _rng.NextDouble() < prof.ErrorRate;
            if (injectErr)
            {
                var status = prof.AbortHttpStatus > 0 ? prof.AbortHttpStatus : StatusCodes.Status503ServiceUnavailable;
                ctx.Response.StatusCode = status;
                var traceId = (string?)ctx.Items.GetValueOrDefault("traceId") ?? $"00-{Guid.NewGuid():N}-01";
                var body = JsonSerializer.Serialize(new { error = "injected_fault", status, traceId });
                _log.LogWarning("Fault injected: {json}", body);
                await ctx.Response.WriteAsync(body);
                return;
            }
        }
        await next(ctx);
    }
}