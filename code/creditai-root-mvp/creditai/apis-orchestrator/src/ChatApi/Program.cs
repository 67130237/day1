using Guardrails.Core;
using Integrations.Mcp;
using Integrations.OpenRouter;
using Integrations.SemanticKernel;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions;
using Shared.Infrastructure;
using Orchestrator.Core;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Services.AddSerilogMinimal(builder.Configuration);

// External clients
OpenRouterClient.Add(builder.Services, builder.Configuration);
McpClientBase.Add(builder.Services); // for MCPs

// Guards
builder.Services.AddSingleton<IRbacGuard, RbacGuard>();
builder.Services.AddSingleton<IPiiMasker, PiiMasker>();

// Kernel facade
builder.Services.AddSingleton<ISkKernelFacade, SkKernelFacade>();

// Orchestrator + Router + Composer
builder.Services.AddSingleton<IIntentRouter, SkIntentRouter>();
builder.Services.AddSingleton<IAnswerComposer, SkAnswerComposer>();
builder.Services.AddSingleton<OrchestratorService>();

// Agents
builder.Services.AddSingleton<IAgent, Agents.SqlAnalyst.SqlAnalystAgent>();
builder.Services.AddSingleton<IAgent, Agents.RagReader.RagReaderAgent>();
builder.Services.AddSingleton<IAgent, Agents.FinancialCalculator.FinancialCalculatorAgent>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.MapPost("/chat/turn", async (
    [FromBody] UserTurn turn,
    OrchestratorService orchestrator,
    CancellationToken ct) =>
{
    var reply = await orchestrator.HandleTurnAsync(turn, ct);
    return Results.Ok(reply);
})
.WithName("chat.turn");

app.Run();
