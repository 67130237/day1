using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Mcp.Rag.Core;

public sealed class QdrantSearch
{
    private readonly RagOptions.QdrantOptions _opt;
    private readonly HttpClient _http = new();

    public QdrantSearch(IOptions<RagOptions> opts)
    {
        _opt = opts.Value.Qdrant;
        _http.BaseAddress = new Uri(_opt.Endpoint.TrimEnd('/') + "/");
        if (!string.IsNullOrWhiteSpace(_opt.ApiKey))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _opt.ApiKey);
    }

    public async Task<List<Passage>> SearchAsync(string query, int topK, object? filters, CancellationToken ct)
    {
        // For MVP, use simple text as "query_vector" via server-side embedding or precomputed; here we assume text search via points/search with a dummy vector (not ideal).
        // In practice, you'd call an embedding model to get the vector. This is a placeholder that uses Qdrant's "search" with a zero vector to return nothing.
        // We'll simulate dense search by calling a hypothetical "recommend" by text payload match (not supported natively). For MVP, return empty and rely on Lexical.
        return new List<Passage>(); // dense stub
    }

    public async Task<string> GetDocAsync(string docId, string sectionId, CancellationToken ct)
    {
        // For MVP, we pretend the content was embedded in payload; return a stub.
        return $"[doc:{docId} section:{sectionId}] (MVP stub content)";
    }
}

public sealed record Passage(string id, string text, double score);
