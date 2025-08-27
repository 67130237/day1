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
app.UseMiddleware<TraceIdRequiredMiddleware>();
app.UseMiddleware<FaultMiddleware>();

app.MapGet("/ping", () => Results.Ok(new { ok = true }));
\
app.MapPost("/xapi/v1/auth/register", async (HttpContext ctx, RegReq req) =>
{
    if (string.IsNullOrWhiteSpace(req.Mobile) || string.IsNullOrWhiteSpace(req.Pin) || string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName) || string.IsNullOrWhiteSpace(req.CitizenId) || req.AcceptTerms != true)
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, "AUTH-REG-VAL", "ข้อมูลสมัครไม่ถูกต้อง");
        return;
    }
    ctx.Response.StatusCode = StatusCodes.Status201Created;
    await ctx.Response.WriteAsJsonAsync(new { userId = Guid.NewGuid().ToString(), registeredAt = DateTimeOffset.Now.ToString("o") });
})
.WithName("AuthRegister")
.Produces(StatusCodes.Status201Created);

app.MapPost("/xapi/v1/auth/login/pin", async (HttpContext ctx, PinLoginReq req) =>
{
    if (string.IsNullOrWhiteSpace(req.Mobile) || string.IsNullOrWhiteSpace(req.Pin) || string.IsNullOrWhiteSpace(req.DeviceId))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, "AUTH-PIN-INVALID", "PIN ไม่ถูกต้อง");
        return;
    }
    var ok = req.Pin == "000000" ? false : true;
    if (!ok)
    {
        await ErrorEnvelope.WriteAsync(ctx, 401, "AUTH-PIN-INVALID", "PIN ไม่ถูกต้อง");
        return;
    }
    await ctx.Response.WriteAsJsonAsync(new { accessToken = "ACCESS", refreshToken = "REFRESH", expiresIn = 3600, mfaRequired = false });
})
.WithName("AuthLoginPin")
.Produces(StatusCodes.Status200OK);

app.MapPost("/xapi/v1/auth/login/biometric", async (HttpContext ctx, BioLoginReq req) =>
{
    if (string.IsNullOrWhiteSpace(req.DeviceId) || string.IsNullOrWhiteSpace(req.Assertion) || string.IsNullOrWhiteSpace(req.Nonce))
    {
        await ErrorEnvelope.WriteAsync(ctx, 401, "AUTH-BIO-INVALID", "Assertion ไม่ถูกต้อง");
        return;
    }
    await ctx.Response.WriteAsJsonAsync(new { accessToken = "ACCESS", refreshToken = "REFRESH", expiresIn = 3600 });
})
.WithName("AuthLoginBio")
.Produces(StatusCodes.Status200OK);

app.Run();

record RegReq(string Mobile, string? Email, string Pin, string FirstName, string LastName, string CitizenId, bool? AcceptTerms, object? Metadata);
record PinLoginReq(string Mobile, string Pin, string DeviceId);
record BioLoginReq(string DeviceId, string Assertion, string Nonce);
