using Elastic.Apm.AspNetCore;
using Microsoft.AspNetCore.Http.Json;
using Project.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration["ServiceName"] = "dashboard-service";
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
app.MapGet("/xapi/v1/dashboard", async (HttpContext ctx, string _) =>
{
    var resp = new {
        cards = new object[] {
            new { type = "balance_summary", accounts = new[]{ new { accountId = "ACC001", available = "1000.00", currency = "THB" } } },
            new { type = "recent_txn", items = new[]{ new { txnId = Guid.NewGuid().ToString(), amount = "-50.00", desc = "Coffee" } } }
        }
    };
    await ctx.Response.WriteAsJsonAsync(resp);
})
.WithName("DashboardGet")
.Produces(StatusCodes.Status200OK);

app.Run();
