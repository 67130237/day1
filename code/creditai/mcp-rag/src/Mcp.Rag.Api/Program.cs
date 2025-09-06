using Mcp.Rag.Core;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Config
builder.Services.Configure<RagOptions>(builder.Configuration.GetSection("Rag"));

// Core services
builder.Services.AddSingleton<QdrantSearch>();
builder.Services.AddSingleton<LexicalSearch>();
builder.Services.AddSingleton<HybridMerger>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.MapPost("/tools/rag.hybrid", async (
    [FromBody] HybridRequest req,
    QdrantSearch qdrant,
    LexicalSearch lexical,
    HybridMerger merge,
    CancellationToken ct) =>
{
    var dense = await qdrant.SearchAsync(req.query, req.topK ?? 8, req.filters, ct);
    var lex = await lexical.SearchAsync(req.query, req.topK ?? 8, ct);
    var final = merge.Merge(dense, lex, req.topK ?? 8);
    return Results.Ok(new { passages = final.Select(x => x.text).ToArray() });
})
.WithName("rag.hybrid");

app.MapPost("/tools/rag.getDoc", async (
    [FromBody] GetDocRequest req,
    QdrantSearch qdrant,
    CancellationToken ct) =>
{
    var content = await qdrant.GetDocAsync(req.docId, req.sectionId, ct);
    return Results.Ok(new { content });
})
.WithName("rag.getDoc");

app.Run();

public sealed record HybridRequest(string query, int? topK, object? filters);
public sealed record GetDocRequest(string docId, string sectionId);
