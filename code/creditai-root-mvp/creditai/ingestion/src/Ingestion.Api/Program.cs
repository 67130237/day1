using Ingestion.Worker.Pipeline.Parsers;
using Ingestion.Worker.Pipeline.Cleaners;
using Ingestion.Worker.Pipeline.Chunkers;
using Ingestion.Worker.Pipeline.Embedders;
using Ingestion.Worker.Pipeline.Upserters;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IParser, TextLikeParser>();
builder.Services.AddSingleton<IPiiCleaner, SimplePiiCleaner>();
builder.Services.AddSingleton<IChunker, SimpleChunker>();
builder.Services.AddSingleton<IEmbedder, OpenRouterEmbedder>();
builder.Services.AddSingleton<IUpserter, QdrantUpserter>();

var app = builder.Build();

app.MapPost("/ingest", async ([FromBody] IngestRequest req,
    IParser parser, IPiiCleaner cleaner, IChunker chunker, IEmbedder embedder, IUpserter upserter, CancellationToken ct) =>
{
    var paths = req.paths ?? Array.Empty<string>();
    var totals = 0;
    foreach (var path in paths)
    {
        if (Directory.Exists(path))
        {
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                 .Where(f => f.EndsWith(".txt") || f.EndsWith(".md"));
            foreach (var file in files)
            {
                var text = await parser.ParseAsync(file, ct);
                text = cleaner.Scrub(text);
                var chunks = chunker.Chunk(text);
                var vectors = await embedder.EmbedBatchAsync(chunks, ct);
                await upserter.UpsertAsync(file, chunks, vectors, ct);
                totals += chunks.Count;
            }
        }
    }
    return Results.Ok(new { ingestedChunks = totals });
});

app.Run();

public sealed record IngestRequest(string[] paths);
