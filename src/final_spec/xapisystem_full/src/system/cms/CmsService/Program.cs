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
app.MapGet("/xapi/v1/cms/home", async (HttpContext ctx) =>
{
    var segment = ctx.Request.Query["segment"].ToString();
    var resp = new
    {
        banners = new[] {
            new { id = Guid.NewGuid().ToString(), title = "Welcome", imageUrl = "https://example.com/banner1.png", actionUrl = "https://example.com/action" }
        },
        sections = new object[] { }
    };
    await ctx.Response.WriteAsJsonAsync(resp);
})
.WithName("CmsHome")
.Produces(StatusCodes.Status200OK);

app.MapGet("/xapi/v1/cms/banners", async (HttpContext ctx) =>
{
    var prefix = FaultParser.ServicePrefix("cms");
    var position = ctx.Request.Query["position"].ToString();
    if (string.IsNullOrWhiteSpace(position))
    {
        await ErrorEnvelope.WriteAsync(ctx, 400, $"{prefix}-BNR-VAL", "position ไม่ถูกต้อง");
        return;
    }

    var resp = new { items = new[] { new { id = Guid.NewGuid().ToString(), position, title = "Top Banner", imageUrl = "https://example.com/banner-top.png", actionUrl = "https://example.com/action" } } };
    await ctx.Response.WriteAsJsonAsync(resp);
})
.WithName("CmsBanners")
.Produces(StatusCodes.Status200OK);

app.Run();
