using Elastic.Apm.AspNetCore;
using Microsoft.AspNetCore.Http.Json;
using Project.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ServiceName"] = "dopa-service";
var ServiceName = builder.Configuration["ServiceName"];
builder.Services.AddElasticsearchLogging($"request-{ServiceName}");

builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

app.UseElasticApm(builder.Configuration);
app.UseRequestLogging();
app.UseMiddleware<SourceHeaderMiddleware>();
app.UseMiddleware<FaultMiddleware>();

app.MapGet("/ping", () => Results.Ok(new { ok = true }));
app.MapPost("/sapi/v1/dopa/verify", async (HttpContext ctx, DopaVerifyReq req) =>
{
    var prefix = FaultParser.ServicePrefix("dopa");

    if (string.IsNullOrWhiteSpace(req.CitizenId) || req.CitizenId.Length != 13
        || string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName)
        || string.IsNullOrWhiteSpace(req.BirthDate))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, $"{prefix}-VAL", "ข้อมูลไม่ครบ/ไม่ถูกต้อง");
        return;
    }

    bool matched = !req.CitizenId.EndsWith("0");
    var score = matched ? 0.98 : 0.12;
    var resp = new { verified = matched, score, provider = "DOPA", checkedAt = DateTimeOffset.Now.ToString("o") };

    await ctx.Response.WriteAsJsonAsync(resp);
})
.WithName("DopaVerify")
.Produces(StatusCodes.Status200OK);

app.Run();

record DopaVerifyReq(string CitizenId, string FirstName, string LastName, string BirthDate, string? LaserCode);
