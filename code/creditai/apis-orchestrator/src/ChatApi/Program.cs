using ChatApi.Guardrails;
using ChatApi.Mcp;
using ChatApi.Orchestrator;
using ChatApi.SemanticKernel;
using ChatApi.Shared.Abstractions;
using ChatApi.Shared.Infrastructure;


using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Services.AddSerilogMinimal(builder.Configuration);

// External clients
McpClientBase.Add(builder.Services); // for MCPs

// Guards
builder.Services.AddSingleton<IRbacGuard, RbacGuard>();
builder.Services.AddSingleton<IPiiMasker, PiiMasker>();


// Kernel facade
builder.Services.AddHttpClient("openrouter", c =>
{
    var baseUrl = builder.Configuration["OpenRouter2:BaseUrl"] ?? "https://openrouter.ai/api/v1/";
    c.BaseAddress = new Uri(baseUrl);
    var key = builder.Configuration["OpenRouter2:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
             ?? throw new InvalidOperationException("OpenRouter API key is missing");
    c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);

    // Optional (บางผู้ให้บริการแนะนำให้ใส่)
    c.DefaultRequestHeaders.Add("HTTP-Referer", "https://your-app.example");
    c.DefaultRequestHeaders.Add("X-Title", "CreditAI");
});

// SK Kernel (แชร์ HttpClient เข้า OpenAI connector)
builder.Services.AddSingleton<Kernel>(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("openrouter");
    var modelChat = builder.Configuration["OpenRouter2:Models:RouterIntent"] ?? "openai/gpt-4o-mini";
    var modelEmbed = builder.Configuration["OpenRouter2:Models:Embedding"] ?? "openai/text-embedding-3-small";
    var apiKey = builder.Configuration["OpenRouter2:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
                 ?? throw new InvalidOperationException("OpenRouter API key is missing");

    var kb = Kernel.CreateBuilder();

    // Chat completions service (OpenAI-compatible) via OpenRouter
    kb.AddOpenAIChatCompletion(modelId: modelChat, apiKey: apiKey, httpClient: http);

    // Embeddings service (OpenAI-compatible) via OpenRouter
    kb.AddOpenAITextEmbeddingGeneration(modelId: modelEmbed, apiKey: apiKey, httpClient: http);

    return kb.Build();
});

// Facade ที่ห่อ Kernel จริง
builder.Services.AddSingleton<SkKernelFacade>();
//builder.Services.AddSingleton<ISkKernelFacade, SkKernelFacadeManual>();

// Orchestrator + Router + Composer
builder.Services.AddSingleton<IPromptRepository, FilePromptRepository>();
builder.Services.AddSingleton<IPlanner, SkPlanner>();
builder.Services.AddSingleton<IIntentRouter, SkIntentRouter>();
builder.Services.AddSingleton<IAnswerComposer, SkAnswerComposer>();
builder.Services.AddSingleton<OrchestratorService>();

// Agents
builder.Services.AddSingleton<IAgent, ChatApi.Agents.SqlAnalyst.SqlAnalystAgent>();
builder.Services.AddSingleton<IAgent, ChatApi.Agents.RagReader.RagReaderAgent>();
builder.Services.AddSingleton<IAgent, ChatApi.Agents.FinancialCalculator.FinancialCalculatorAgent>();

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


//https://github.com/modelcontextprotocol/csharp-sdk/tree/main/samples
