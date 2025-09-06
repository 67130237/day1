using CreditAI.Modules.Integrations.Mcp;
using System.Text.Json.Nodes;

namespace CreditAI.Modules.Rag;

public sealed class RagMcpClient : IRagMcpClient
{
    private readonly IMcpClient _client;
    public RagMcpClient(IMcpClient client) => _client = client;

    public Task<JsonNode?> SearchAsync(string query, int k, CancellationToken ct)
    {
        var args = new JsonObject { ["query"] = query, ["k"] = k };
        return _client.CallToolAsync("rag.search", args, ct);
    }
}
