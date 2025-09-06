namespace ChatApi.Mcp;

public sealed class McpRagToolClient : McpClientBase
{
    public McpRagToolClient(IHttpClientFactory factory, IConfiguration cfg)
        : base(factory, cfg, "Mcp:Rag")
    {
    }

    public async Task<IReadOnlyList<string>> HybridSearchAsync(string query, int topK = 8, object? filters = null, CancellationToken ct = default)
    {
        using var http = CreateClient();
        var payload = new { query, topK, filters };
        var resp = await http.PostAsync("tools/rag.hybrid", Json(payload), ct);
        var obj = await ReadJsonAsync<HybridResponse>(resp, ct);
        return obj.passages ?? Array.Empty<string>();
    }

    public async Task<string> GetDocAsync(string docId, string sectionId, CancellationToken ct = default)
    {
        using var http = CreateClient();
        var payload = new { docId, sectionId };
        var resp = await http.PostAsync("tools/rag.getDoc", Json(payload), ct);
        var obj = await ReadJsonAsync<GetDocResponse>(resp, ct);
        return obj.content ?? string.Empty;
    }

    private sealed class HybridResponse { public string[]? passages { get; set; } }
    private sealed class GetDocResponse { public string? content { get; set; } }
}
