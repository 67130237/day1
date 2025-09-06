using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace CreditAI.Modules.Integrations.Mcp;

public interface IMcpClient
{
    Task<IReadOnlyList<string>> ListToolsAsync(CancellationToken ct);
    Task<JsonNode?> CallToolAsync(string toolName, JsonNode? args, CancellationToken ct);
}
