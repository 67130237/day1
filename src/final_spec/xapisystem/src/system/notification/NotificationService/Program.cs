\
using Elastic.Apm.AspNetCore;
using Microsoft.AspNetCore.Http.Json;
using Project.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ServiceName"] = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "unknown";

builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

app.UseElasticApm(builder.Configuration);
app.UseMiddleware<SourceHeaderMiddleware>();
app.UseMiddleware<FaultMiddleware>();

app.MapGet("/ping", () => Results.Ok(new { ok = true }));
\
app.MapPost("/papi/v1/notifications", async (HttpContext ctx, NotiReq req) =>
{
    var prefix = FaultParser.ServicePrefix("notification");

    if (req.To is null || (string.IsNullOrWhiteSpace(req.To.UserId) && string.IsNullOrWhiteSpace(req.To.Topic)) || string.IsNullOrWhiteSpace(req.Template))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, $"{prefix}-VAL", "รูปแบบไม่ถูกต้อง");
        return;
    }

    var resp = new { requestId = Guid.NewGuid().ToString(), accepted = true };
    ctx.Response.StatusCode = StatusCodes.Status202Accepted;
    await ctx.Response.WriteAsJsonAsync(resp);
})
.WithName("NotiSend")
.Produces(StatusCodes.Status202Accepted);

app.Run();

record NotiReq(ToObj To, string Template, Dictionary<string,object>? Data, string? DedupKey, string? ScheduleAt);
record ToObj(string? UserId, string? Topic);
