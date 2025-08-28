using Elastic.Apm.AspNetCore;
using Microsoft.AspNetCore.Http.Json;
using Project.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration["ServiceName"] = "transaction-service";
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
app.UseMiddleware<TraceIdRequiredMiddleware>();
app.UseMiddleware<FaultMiddleware>();

app.MapGet("/ping", () => Results.Ok(new { ok = true }));

app.MapPost("/xapi/v1/transfers/quote", async (HttpContext ctx, QuoteReq req) =>
{
    if (string.IsNullOrWhiteSpace(req.SourceAccountId) || req.Dest is null || string.IsNullOrWhiteSpace(req.Amount) || string.IsNullOrWhiteSpace(req.Currency))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, "TXN-QUOTE-VAL", "ข้อมูลไม่ถูกต้อง");
        return;
    }
    if (decimal.TryParse(req.Amount, out var amt) && amt > 100000)
    {
        await ErrorEnvelope.WriteAsync(ctx, 409, "TXN-QUOTE-LIMIT", "เกินวงเงิน");
        return;
    }
    var resp = new { fee = "10.00", currency = "THB", limitOk = true, amlFlag = false, message = "" };
    await ctx.Response.WriteAsJsonAsync(resp);
})
.WithName("TransferQuote")
.Produces(StatusCodes.Status200OK);

app.MapPost("/xapi/v1/transfers", async (HttpContext ctx, TransferReq req) =>
{
    var idem = ctx.Request.Headers["Idempotency-Key"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(idem))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, "TXN-XFER-VAL", "Idempotency-Key required");
        return;
    }
    if (idem == "DUP")
    {
        await ErrorEnvelope.WriteAsync(ctx, 409, "TXN-XFER-DUP", "ทำซ้ำ");
        return;
    }
    if (string.IsNullOrWhiteSpace(req.SourceAccountId) || req.Dest is null || string.IsNullOrWhiteSpace(req.Amount) || string.IsNullOrWhiteSpace(req.Currency))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, "TXN-XFER-VAL", "ข้อมูลไม่ถูกต้อง");
        return;
    }
    await ctx.Response.WriteAsJsonAsync(new { transferId = Guid.NewGuid().ToString(), status = "SUCCESS", postedAt = DateTimeOffset.Now.ToString("o") });
    ctx.Response.StatusCode = StatusCodes.Status201Created;
})
.WithName("TransferCreate")
.Produces(StatusCodes.Status201Created);

app.MapPost("/xapi/v1/withdrawals", async (HttpContext ctx, WithdrawalReq req) =>
{
    var idem = ctx.Request.Headers["Idempotency-Key"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(idem))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, "TXN-WD-VAL", "Idempotency-Key required");
        return;
    }
    if (idem == "DUP")
    {
        await ErrorEnvelope.WriteAsync(ctx, 409, "TXN-WD-SYS", "ทำซ้ำ");
        return;
    }
    await ctx.Response.WriteAsJsonAsync(new { withdrawalId = Guid.NewGuid().ToString(), status = "issued", token = "ABC123", expireAt = DateTimeOffset.Now.AddMinutes(10).ToString("o") });
    ctx.Response.StatusCode = StatusCodes.Status201Created;
})
.WithName("WithdrawalCreate")
.Produces(StatusCodes.Status201Created);

app.Run();

record QuoteReq(string SourceAccountId, DestObj Dest, string Amount, string Currency);
record DestObj(string AccountNo, string BankCode, string Name);
record TransferReq(string SourceAccountId, object Dest, string Amount, string Currency, string? Note);
record WithdrawalReq(string SourceAccountId, string Amount, string Currency, string Channel);
