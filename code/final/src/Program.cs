using CreditAI.Modules.Agents;
using CreditAI.Modules.Guardrails;
using CreditAI.Modules.Integrations.Mcp;
using CreditAI.Modules.LLM;
using CreditAI.Modules.Rag;
using CreditAI.Orchestrator.Modules.Orchestration;
using CreditAI.Shared.Contracts;
using CreditAI.Shared.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Route = CreditAI.Modules.Agents.Route;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Services.AddSerilog(cfg => cfg.WriteTo.Console());
builder.Services.AddEndpointsApiExplorer();

// SK
builder.Services.AddSkKernel(builder.Configuration);

// HTTP clients for MCP (HTTP mode)
builder.Services.AddResilientHttpClient("mcp-mssql-http");
builder.Services.AddResilientHttpClient("mcp-rag-http");

// MCP wiring (configurable transport)
// MSSQL
builder.Services.AddSingleton<IMcpClient>(sp =>
{
    var cfg = builder.Configuration.GetSection("mcp:mssql").Get<CreditAI.Modules.Integrations.Mcp.McpClientOptions>()!;
    if (cfg.Transport == "http" && cfg.Http?.BaseUrl is { } url)
        return new HttpMcpClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("mcp-mssql-http"), url);
    return new StdioMcpClient(cfg); // TODO: implement stdio protocol
});
builder.Services.AddSingleton<IMssqlMcpClient, MssqlMcpClient>(sp => new MssqlMcpClient(sp.GetRequiredService<IMcpClient>()));

// RAG
builder.Services.AddSingleton<IMcpClient>(sp =>
{
    var cfg = builder.Configuration.GetSection("mcp:rag").Get<CreditAI.Modules.Integrations.Mcp.McpClientOptions>()!;
    if (cfg.Transport == "http" && cfg.Http?.BaseUrl is { } url)
        return new HttpMcpClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient("mcp-rag-http"), url);
    return new StdioMcpClient(cfg); // TODO: implement stdio protocol
});
builder.Services.AddSingleton<IRagMcpClient, RagMcpClient>();

// Agents
builder.Services.AddSingleton<SqlAnalystAgent>();
builder.Services.AddSingleton<RagReaderAgent>();
builder.Services.AddSingleton<IntentRouter>();
builder.Services.AddSingleton<AnswerComposer>();

// Register Orchestrator service
builder.Services.AddSingleton<Orchestrator>();

var app = builder.Build();

// Guardrails
app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<PiiMaskingMiddleware>();

app.MapGet("/health", () => Results.Ok(new { ok = true }));

app.MapPost("/agents/sql:run", async ([FromBody] SqlRunRequest req, IMssqlMcpClient mssql, CancellationToken ct) =>
{
    var r = await mssql.ExecuteSqlAsync(req.Sql, req.Top, req.TimeoutSec, ct);
    return Results.Ok(r);
});

app.MapPost("/rag/search", async ([FromBody] RagSearchRequest req, IRagMcpClient rag, CancellationToken ct) =>
{
    var r = await rag.SearchAsync(req.Query, req.K, ct);
    return Results.Ok(r);
});

app.MapPost("/rag/ingest", async ([FromBody] RagIngestRequest req) =>
{
    return Results.Ok(new { ok = true, message = "TODO: implement ingest pipeline", id = req.Id, len = req.Text?.Length ?? 0 });
});

// === The main chat endpoint ===
// BOTH: run RAG first (to gather evidence) -> pass evidence to SQL agent -> compose
// === The main chat endpoint (เรียก service) ===
app.MapPost("/chat", async ([FromBody] ChatRequest req, Orchestrator orchestrator, CancellationToken ct) =>
{
    var resp = await orchestrator.ProcessAsync(req, ct);
    return Results.Ok(resp);
});
//app.MapPost("/chat", async ([FromBody] ChatRequest req,
//    IntentRouter router, SqlAnalystAgent sql, RagReaderAgent rag, AnswerComposer composer, CancellationToken ct) =>
//{
//    var (route, notes) = await router.RouteAsync(req.Text, ct);
//    var drafts = new List<AgentDraft>();

//    if (route == Route.BOTH)
//    {
//        var ragDraft = await rag.HandleAsync(req.Text, ct);
//        drafts.Add(ragDraft);

//        var sqlDraft = await sql.HandleAsync(req.Text, ct, ragDraft.Evidence);
//        drafts.Add(sqlDraft);
//    }
//    else if (route == Route.SQL_ANALYST)
//    {
//        drafts.Add(await sql.HandleAsync(req.Text, ct));
//    }
//    else // RAG_READER
//    {
//        drafts.Add(await rag.HandleAsync(req.Text, ct));
//    }

//    var answer = await composer.ComposeAsync(req.Text, drafts, ct);
//    return Results.Ok(new ChatResponse(
//        answer,
//        drafts.SelectMany(d => d.Evidence).ToList(),
//        new Dictionary<string, string> { { "route", route.ToString() }, { "notes", notes } }
//    ));
//});

app.MapGet("/tools", () => Results.Ok(new[] { "mssql.execute_sql", "mssql.list_tables", "mssql.describe_table", "rag.search" }));

app.Run();
