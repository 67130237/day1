using Elastic.Apm.AspNetCore;
using Microsoft.AspNetCore.Http.Json;
using Project.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ServiceName"] = "appsettings-service";
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

app.MapGet("/xapi/v1/appsettings", async (HttpContext ctx, string _) =>
{
    var prefix = FaultParser.ServicePrefix("appsettings");
    var scope = ctx.Request.Query["scope"].ToString();
    if (!string.Equals(scope, "public", StringComparison.OrdinalIgnoreCase))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, $"{prefix}-VAL", "scope ไม่รองรับ");
        return; 
    }
    else
    {
        var resp = new { minVersion = "1.2.3", featureFlags = new { transfer_otp = true } };
        await ctx.Response.WriteAsJsonAsync(resp);
    }
})
.WithName("AppSettingsGet")
.Produces(StatusCodes.Status200OK);

app.Run();
