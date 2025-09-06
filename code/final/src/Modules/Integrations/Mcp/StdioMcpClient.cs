using System.Text.Json.Nodes;

namespace CreditAI.Modules.Integrations.Mcp;

public sealed class StdioMcpClient : IMcpClient
{
    private readonly McpClientOptions _opts;
    public StdioMcpClient(McpClientOptions opts) => _opts = opts;

    public Task<IReadOnlyList<string>> ListToolsAsync(CancellationToken ct)
        => Task.FromResult((IReadOnlyList<string>)new List<string>());

    public Task<JsonNode?> CallToolAsync(string toolName, JsonNode? args, CancellationToken ct)
        => throw new NotImplementedException("TODO: Implement MCP stdio protocol handshake + tool.call per MCP spec.");
}
