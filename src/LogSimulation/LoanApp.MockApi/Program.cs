using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Services;
using LoanApp.MockApi.Dtos;
using LoanApp.MockApi.Middleware;
using LoanApp.MockApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// core services
builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddSingleton<FaultRegistry>();
builder.Services.AddSingleton<IdempotencyCache>();
builder.Services.AddSingleton<EventQueues>();

builder.Services.AddTransient<RequestLoggingMiddleware>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddTransient<FaultInjectionMiddleware>();

builder.Services.AddHttpClient("self", c =>
{
    c.BaseAddress = new Uri("http://localhost:5080");
});

builder.Services.AddHostedService<WebhookEmulator>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// middlewares
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<FaultInjectionMiddleware>();

app.MapControllers();

// admin endpoints for faults/scenario
app.MapPost("/_admin/faults/load", async (FaultRegistry reg, HttpRequest req) =>
{
    var cfg = await req.ReadFromJsonAsync<FaultConfig>();
    if (cfg is null) return Results.BadRequest("invalid config");
    reg.Load(cfg);
    return Results.Ok(new { ok = true });
});

app.MapPost("/_admin/faults/toggle/{enabled:bool}", (FaultRegistry reg, bool enabled) =>
{
    reg.Toggle(enabled);
    return Results.Ok(new { enabled });
});

app.MapGet("/_admin/faults", (FaultRegistry reg) => reg.Snapshot());

app.Run();