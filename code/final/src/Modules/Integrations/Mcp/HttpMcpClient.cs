using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace CreditAI.Modules.Integrations.Mcp;

public sealed class HttpMcpClient : IMcpClient
{
    private readonly HttpClient _http;
    private readonly string _base;
    public HttpMcpClient(HttpClient http, string baseUrl)
    {
        _http = http;
        _base = baseUrl.TrimEnd('/');
    }

    public async Task<IReadOnlyList<string>> ListToolsAsync(CancellationToken ct)
    {
        var res = await _http.GetFromJsonAsync<string[]>($"{_base}/tools", ct) ?? Array.Empty<string>();
        return res;
    }

    public async Task<JsonNode?> CallToolAsync(string toolName, JsonNode? args, CancellationToken ct)
    {
        var payload = new { tool = toolName, args = args };
        var res = await _http.PostAsJsonAsync($"{_base}/call", payload, ct);
        res.EnsureSuccessStatusCode();
        var node = await res.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: ct);
        return node;
    }
}
