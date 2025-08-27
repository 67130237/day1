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
app.MapGet("/xapi/v1/customers/me", async (HttpContext ctx) =>
{
    var resp = new { userId = "U001", fullName = "Jane Doe", mobile = "+66812345678", email = "jane@example.com", kycLevel = "basic",
        preferences = new { lang = "th-TH", marketing = false } };
    await ctx.Response.WriteAsJsonAsync(resp);
})
.WithName("CustomerMe")
.Produces(StatusCodes.Status200OK);

app.MapPut("/xapi/v1/customers/me/settings", async (HttpContext ctx, CustSettingsReq req) =>
{
    if (req.Preferences is null)
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, "CUST-SET-VAL", "รูปแบบข้อมูลไม่ถูกต้อง");
        return;
    }
    await ctx.Response.WriteAsJsonAsync(new { updated = true });
})
.WithName("CustomerSettings")
.Produces(StatusCodes.Status200OK);

app.Run();

record CustSettingsReq(PreferencesObj Preferences);
record PreferencesObj(string Lang, bool Marketing);
