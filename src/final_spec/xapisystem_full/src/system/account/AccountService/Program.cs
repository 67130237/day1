using Elastic.Apm.AspNetCore;
using Microsoft.AspNetCore.Http.Json;
using Project.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration["ServiceName"] = "account-service";
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

app.MapGet("/xapi/v1/accounts", async (HttpContext ctx) =>
{
    var resp = new {
        items = new[] {
            new { accountId = "ACC001", type = "SAVING", numberMasked = "xxx-xxx-1234", currency = "THB", status = "ACTIVE" }
        },
        nextPageToken = (string?)null
    };
    return Results.Ok(resp);
})
.WithName("AccList")
.Produces(StatusCodes.Status200OK);

app.MapGet("/xapi/v1/accounts/{accountId}/balance", async (HttpContext ctx, string accountId) =>
{
    if (string.IsNullOrWhiteSpace(accountId))
    {
        await ErrorEnvelope.WriteAsync(ctx, 404, "ACC-BAL-404", "ไม่พบบัญชี");
        return;
    }
    var resp = new { accountId, available = "1000.00", ledger = "1000.00", currency = "THB", asOf = DateTimeOffset.Now.ToString("o") };

    await ctx.Response.WriteAsJsonAsync(resp);
})
.WithName("AccBalance")
.Produces(StatusCodes.Status200OK);

app.MapGet("/xapi/v1/accounts/{accountId}/transactions", async (HttpContext ctx, string accountId) =>
{
    if (string.IsNullOrWhiteSpace(accountId))
    {
        await ErrorEnvelope.WriteAsync(ctx, 404, "ACC-TXN-404", "ไม่พบบัญชี");
        return;
    }
    var resp = new {
        items = new[] { new { txnId = Guid.NewGuid().ToString(), postedAt = DateTimeOffset.Now.ToString("o"), amount = "-100.00", currency = "THB", type = "DEBIT", desc = "Transfer to ..." } },
        nextPageToken = (string?)null
    };
    await ctx.Response.WriteAsJsonAsync(resp);
})
.WithName("AccTransactions")
.Produces(StatusCodes.Status200OK);

app.Run();
