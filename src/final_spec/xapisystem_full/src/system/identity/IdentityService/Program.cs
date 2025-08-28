using Elastic.Apm.AspNetCore;
using Microsoft.AspNetCore.Http.Json;
using Project.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration["ServiceName"] = "identity-service";
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
//app.UseMiddleware<TraceIdRequiredMiddleware>();
app.UseMiddleware<FaultMiddleware>();

app.MapGet("/ping", () => Results.Ok(new { ok = true }));
app.MapPost("/papi/v1/identity", async (HttpContext ctx, IdentityCreateReq req) =>
{
    if (string.IsNullOrWhiteSpace(req.UserId))
    {
        await ErrorEnvelope.WriteAsync(ctx, 404, "IDEN-UPD-404", "ไม่พบผู้ใช้");
        return;
    }
    var kyc = string.IsNullOrWhiteSpace(req.KycLevel) ? "basic" : req.KycLevel;
    ctx.Response.StatusCode = StatusCodes.Status201Created;
    await ctx.Response.WriteAsJsonAsync(new { identityId = Guid.NewGuid().ToString(), userId = req.UserId, kycLevel = kyc, activatedAt = DateTimeOffset.Now.ToString("o") });
})
.WithName("IdentityCreate")
.Produces(StatusCodes.Status201Created);

app.MapPost("/papi/v1/identity/verify-dopa", async (HttpContext ctx, DopaVerifyReq req) =>
{
    if (string.IsNullOrWhiteSpace(req.CitizenId) || string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName) || string.IsNullOrWhiteSpace(req.BirthDate))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, "IDEN-DOPA-VAL", "รูปแบบข้อมูลไม่ถูกต้อง");
        return;
    }
    bool matched = !req.CitizenId.EndsWith("0");
    var matchedFields = matched ? new[] { "firstName", "lastName", "birthDate" } : Array.Empty<string>();
    await ctx.Response.WriteAsJsonAsync(new { verified = matched, matchedFields });
})
.WithName("IdentityVerifyDopa")
.Produces(StatusCodes.Status200OK);

app.Run();

record IdentityCreateReq(string UserId, string? KycLevel);
record DopaVerifyReq(string CitizenId, string FirstName, string LastName, string BirthDate, string? LaserCode);
