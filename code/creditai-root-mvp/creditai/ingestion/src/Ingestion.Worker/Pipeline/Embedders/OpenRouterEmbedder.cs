using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ingestion.Worker.Pipeline.Embedders;

public interface IEmbedder
{
    Task<List<float[]>> EmbedBatchAsync(List<string> texts, CancellationToken ct);
}

public sealed class OpenRouterEmbedder : IEmbedder
{
    private readonly IHttpClientFactory _hf;
    private readonly IConfiguration _cfg;
    public OpenRouterEmbedder(IHttpClientFactory hf, IConfiguration cfg) { _hf = hf; _cfg = cfg; }

    public async Task<List<float[]>> EmbedBatchAsync(List<string> texts, CancellationToken ct)
    {
        var apiKey = _cfg["OpenRouter:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("OpenRouter API key missing");

        var http = _hf.CreateClient();
        http.BaseAddress = new Uri(_cfg["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var model = _cfg["OpenRouter:EmbeddingModel"] ?? "text-embedding-3-small";

        var results = new List<float[]>();
        foreach (var text in texts)
        {
            var payload = new { model, input = text };
            var json = JsonSerializer.Serialize(payload);
            var resp = await http.PostAsync("embeddings", new StringContent(json, Encoding.UTF8, "application/json"), ct);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);
            var arr = doc.RootElement.GetProperty("data")[0].GetProperty("embedding").EnumerateArray().Select(x => x.GetSingle()).ToArray();
            results.Add(arr);
        }
        return results;
    }
}
