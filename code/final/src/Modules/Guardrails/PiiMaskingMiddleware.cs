using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace CreditAI.Modules.Guardrails;

public class PiiMaskingMiddleware
{
    private readonly RequestDelegate _next;
    public PiiMaskingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx)
    {
        var originalBody = ctx.Response.Body;
        using var mem = new MemoryStream();
        ctx.Response.Body = mem;

        await _next(ctx);

        mem.Position = 0;
        var text = await new StreamReader(mem, Encoding.UTF8).ReadToEndAsync();
        var masked = PiiMasking.Mask(text);
        var bytes = Encoding.UTF8.GetBytes(masked);
        ctx.Response.Body = originalBody;
        ctx.Response.ContentLength = bytes.Length;
        await ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }
}
