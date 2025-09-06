using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Ingestion.Worker.Pipeline.Upserters;

public interface IUpserter
{
    Task EnsureCollectionAsync(CancellationToken ct);
    Task UpsertAsync(string sourcePath, List<string> chunks, List<float[]> vectors, CancellationToken ct);
}

public sealed class QdrantUpserter : IUpserter
{
    private readonly HttpClient _http = new();
    private readonly RagOptions _opt;
    private readonly IConfiguration _cfg;

    public QdrantUpserter(IConfiguration cfg)
    {
        _cfg = cfg;
        _opt = new RagOptions
        {
            Endpoint = cfg["Rag:Qdrant:Endpoint"] ?? "http://localhost:6333",
            Collection = cfg["Rag:Qdrant:Collection"] ?? "creditai-docs",
            ApiKey = cfg["Rag:Qdrant:ApiKey"]
        };
        _http.BaseAddress = new Uri(_opt.Endpoint.TrimEnd('/') + "/");
        if (!string.IsNullOrWhiteSpace(_opt.ApiKey))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _opt.ApiKey);
    }

    public async Task EnsureCollectionAsync(CancellationToken ct)
    {
        var dims = int.Parse(_cfg["Rag:Qdrant:Dims"] ?? "1536");
        var distance = _cfg["Rag:Qdrant:Distance"] ?? "Cosine";
        var payload = new
        {
            vectors = new { size = dims, distance = distance },
            on_disk_payload = true
        };
        var json = JsonSerializer.Serialize(payload);
        var resp = await _http.PutAsync($"collections/{_opt.Collection}", new StringContent(json, Encoding.UTF8, "application/json"), ct);
        if (!resp.IsSuccessStatusCode)
        {
            var b = await resp.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Qdrant create collection failed: {resp.StatusCode} - {b}");
        }
    }

    public async Task UpsertAsync(string sourcePath, List<string> chunks, List<float[]> vectors, CancellationToken ct)
    {
        var points = new List<object>();
        for (int i = 0; i < chunks.Count; i++)
        {
            points.Add(new
            {
                id = Guid.NewGuid().ToString("N"),
                vector = vectors[i],
                payload = new
                {
                    source = sourcePath,
                    section = i,
                    text = chunks[i]
                }
            });
        }
        var body = JsonSerializer.Serialize(new { points });
        var resp = await _http.PutAsync($"collections/{_opt.Collection}/points", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        resp.EnsureSuccessStatusCode();
    }

    private sealed class RagOptions
    {
        public string Endpoint { get; set; } = "http://localhost:6333";
        public string Collection { get; set; } = "creditai-docs";
        public string? ApiKey { get; set; }
    }
}
