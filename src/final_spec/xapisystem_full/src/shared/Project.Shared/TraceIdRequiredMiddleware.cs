
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Project.Shared;

public class TraceIdRequiredMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _serviceName;
    public TraceIdRequiredMiddleware(RequestDelegate next, IConfiguration cfg)
    {
        _next = next;
        _serviceName = cfg["ServiceName"] ?? Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "unknown";
    }

    public async Task Invoke(HttpContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.Request.Headers["xapp-trace-id"].FirstOrDefault()))
        {
            var pfx = FaultParser.ServicePrefix(_serviceName);
            await ErrorEnvelope.WriteAsync(ctx, 400, $"{pfx}-VAL", "xapp-trace-id required");
            return;
        }
        await _next(ctx);
    }
}
