using System.Text.Json;

namespace LoanApp.MockApi.Middleware;

public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _log;
    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> log) => _log = log;

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            var traceId = (string?)ctx.Items.GetValueOrDefault("traceId") ?? $"00-{Guid.NewGuid():N}-01";
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            ctx.Response.ContentType = "application/json";
            var payload = JsonSerializer.Serialize(new { error = "server_error", message = ex.Message, traceId });
            _log.LogError(ex, payload);
            await ctx.Response.WriteAsync(payload);
        }
    }
}