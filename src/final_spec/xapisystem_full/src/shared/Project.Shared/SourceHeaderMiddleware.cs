\
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Project.Shared;

public class SourceHeaderMiddleware
{
    private readonly RequestDelegate _next;
    public SourceHeaderMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        ctx.Response.OnStarting(() =>
        {
            ErrorEnvelope.EnsureSourceHeader(ctx);
            return Task.CompletedTask;
        });
        await _next(ctx);
    }
}
