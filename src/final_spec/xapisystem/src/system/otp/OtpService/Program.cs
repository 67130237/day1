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
app.MapPost("/sapi/v1/otp/send", async (HttpContext ctx, OtpSendReq req) =>
{
    var prefix = FaultParser.ServicePrefix("otp");

    if (string.IsNullOrWhiteSpace(req.To) || string.IsNullOrWhiteSpace(req.Channel) || string.IsNullOrWhiteSpace(req.Purpose))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, $"{prefix}-SEND-DEST", "ที่อยู่ปลายทางหรือข้อมูลไม่ถูกต้อง");
        return;
    }

    int ttl = req.TtlSec ?? 300;
    var resp = new { otpRefId = Guid.NewGuid().ToString(), expireAt = DateTimeOffset.Now.AddSeconds(ttl).ToString("o") };
    await ctx.Response.WriteAsJsonAsync(resp);
})
.WithName("OtpSend")
.Produces(StatusCodes.Status200OK);

app.MapPost("/sapi/v1/otp/verify", async (HttpContext ctx, OtpVerifyReq req) =>
{
    var prefix = FaultParser.ServicePrefix("otp");
    if (string.IsNullOrWhiteSpace(req.OtpRefId) || string.IsNullOrWhiteSpace(req.Code))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, $"{prefix}-VERIFY-INVALID", "OTP ไม่ถูกต้อง");
        return;
    }

    if (req.Code == "999999")
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, $"{prefix}-VERIFY-EXPIRED", "OTP หมดอายุ");
        return;
    }

    await ctx.Response.WriteAsJsonAsync(new { verified = true });
})
.WithName("OtpVerify")
.Produces(StatusCodes.Status200OK);

app.Run();

record OtpSendReq(string To, string Channel, string Purpose, int? TtlSec, string? Template);
record OtpVerifyReq(string OtpRefId, string Code);
