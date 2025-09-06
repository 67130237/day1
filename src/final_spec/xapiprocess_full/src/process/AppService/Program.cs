using Elastic.Apm.AspNetCore;
using Microsoft.AspNetCore.Http.Json;
using Project.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration["ServiceName"] = "account-service";
var ServiceName = builder.Configuration["ServiceName"];
builder.Services.AddElasticsearchLogging($"request-{ServiceName}");

// HttpContext + ForwardTrace handler
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TraceIdForwardingHandler>();

builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddDownstreamHttpClients(cfg);

var app = builder.Build();

app.UseElasticApm(builder.Configuration);
app.UseRequestLogging();
app.UseMiddleware<SourceHeaderMiddleware>();
//app.UseMiddleware<TraceIdRequiredMiddleware>();
app.UseMiddleware<FaultMiddleware>();

app.MapGet("/ping", () => Results.Ok(new { ok = true }));

app.MapRegisterProcessEndpoints();

app.Run();
