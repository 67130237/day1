using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace CreditAI.Modules.Guardrails;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HashSet<string> _keys;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration cfg)
    {
        _next = next;
        _keys = cfg.GetSection("rbac:apiKeys").Get<string[]>()?.ToHashSet() ?? new();
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (ctx.Request.Path.StartsWithSegments("/health"))
        {
            await _next(ctx);
            return;
        }

        if (!_keys.Any())
        {
            ctx.Response.StatusCode = 500;
            await ctx.Response.WriteAsync("RBAC not configured");
            return;
        }

        var key = ctx.Request.Headers["X-API-Key"].FirstOrDefault();
        if (key is null || !_keys.Contains(key))
        {
            ctx.Response.StatusCode = 401;
            await ctx.Response.WriteAsync("Unauthorized");
            return;
        }

        await _next(ctx);
    }
}
