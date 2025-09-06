using Ingestion.Worker.Pipeline.Parsers;
using Ingestion.Worker.Pipeline.Cleaners;
using Ingestion.Worker.Pipeline.Chunkers;
using Ingestion.Worker.Pipeline.Embedders;
using Ingestion.Worker.Pipeline.Upserters;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IParser, TextLikeParser>();    // MVP: txt/md
builder.Services.AddSingleton<IPiiCleaner, SimplePiiCleaner>();
builder.Services.AddSingleton<IChunker, SimpleChunker>();
builder.Services.AddSingleton<IEmbedder, OpenRouterEmbedder>();
builder.Services.AddSingleton<IUpserter, QdrantUpserter>();

var app = builder.Build();

app.Services.GetRequiredService<IUpserter>().EnsureCollectionAsync(default).GetAwaiter().GetResult();

await app.RunAsync();

public static class HostExtensions
{
    public static async Task RunAsync(this IHost host)
    {
        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Ingestion");
        var parser = host.Services.GetRequiredService<IParser>();
        var cleaner = host.Services.GetRequiredService<IPiiCleaner>();
        var chunker = host.Services.GetRequiredService<IChunker>();
        var embedder = host.Services.GetRequiredService<IEmbedder>();
        var upserter = host.Services.GetRequiredService<IUpserter>();

        var input = Environment.GetEnvironmentVariable("INGEST_INPUT") ?? "./data";
        if (!Directory.Exists(input))
        {
            logger.LogWarning("Input folder not found: {folder}", input);
            return;
        }

        var files = Directory.GetFiles(input, "*.*", SearchOption.AllDirectories)
                             .Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                             .ToArray();
        logger.LogInformation("Found {count} files to ingest", files.Length);

        foreach (var file in files)
        {
            logger.LogInformation("Processing {file}", file);
            var text = await parser.ParseAsync(file, default);
            text = cleaner.Scrub(text);

            var chunks = chunker.Chunk(text);
            var vectors = await embedder.EmbedBatchAsync(chunks, default);

            await upserter.UpsertAsync(file, chunks, vectors, default);
            logger.LogInformation("Upserted {n} chunks from {file}", chunks.Count, file);
        }

        logger.LogInformation("Ingestion finished.");
    }
}
