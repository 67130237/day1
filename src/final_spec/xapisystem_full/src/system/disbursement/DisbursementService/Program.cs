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
app.MapPost("/papi/v1/disbursements/eligibility", async (HttpContext ctx, DisbEligReq req) =>
{
    if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.ProductCode) || string.IsNullOrWhiteSpace(req.Amount) || string.IsNullOrWhiteSpace(req.Currency))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, "DISB-ELIG-VAL", "ข้อมูลไม่ถูกต้อง");
        return;
    }
    decimal amt = decimal.TryParse(req.Amount, out var a) ? a : 0m;
    bool eligible = amt <= 50000;
    var resp = new { eligible, maxAmount = "50000.00", currency = "THB", reason = eligible ? "" : "amount_exceeds_limit" };
    await ctx.Response.WriteAsJsonAsync(resp);
})
.WithName("DisbEligibility")
.Produces(StatusCodes.Status200OK);

app.MapPost("/papi/v1/disbursements", async (HttpContext ctx, DisbStartReq req) =>
{
    var idem = ctx.Request.Headers["Idempotency-Key"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(idem))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, "DISB-START-VAL", "Idempotency-Key required");
        return;
    }
    if (idem == "DUP")
    {
        await ErrorEnvelope.WriteAsync(ctx, 409, "DISB-START-DUP", "ทำซ้ำ");
        return;
    }
    ctx.Response.StatusCode = StatusCodes.Status202Accepted;
    await ctx.Response.WriteAsJsonAsync(new { disbursementId = Guid.NewGuid().ToString(), status = "PROCESSING" });
})
.WithName("DisbStart")
.Produces(StatusCodes.Status202Accepted);

app.Run();

record DisbEligReq(string UserId, string ProductCode, string Amount, string Currency);
record DisbStartReq(string ContractId, string PayToAccountId, string Amount, string Currency);
