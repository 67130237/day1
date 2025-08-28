using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Project.Shared;

public static class ErrorEnvelope
{
    public static async Task WriteAsync(HttpContext ctx, int httpStatus, string code, string message, object? details = null)
    {
        ctx.Response.StatusCode = httpStatus;
        ctx.Response.ContentType = "application/json";
        EnsureSourceHeader(ctx);
        var traceId = ctx.Request.Headers["xapp-trace-id"].FirstOrDefault() ?? ctx.TraceIdentifier;
        var payload = new { code, message, traceId, details = details ?? new { } };
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }

    public static string MapStatusToCode(int status, string pfx) => status switch
    {
        400 => $"{pfx}-VAL",
        401 => $"{pfx}-UNAUTH",
        403 => $"{pfx}-FORBID",
        404 => $"{pfx}-404",
        409 => $"{pfx}-CONFLICT",
        422 => $"{pfx}-UNPROC",
        429 => $"{pfx}-RATE",
        502 => $"{pfx}-UPSTREAM",
        503 => $"{pfx}-SYS",
        _   => $"{pfx}-SYS"
    };

    public static void EnsureSourceHeader(HttpContext ctx)
    {
        if (!ctx.Response.Headers.ContainsKey("xapp-source"))
            ctx.Response.Headers["xapp-source"] = $"{AppInfo.ServiceName}-service@{AppInfo.Version}";
    }
}
