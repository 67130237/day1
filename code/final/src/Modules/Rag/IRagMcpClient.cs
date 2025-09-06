using System.Text.Json.Nodes;

namespace CreditAI.Modules.Rag;

public interface IRagMcpClient
{
    Task<JsonNode?> SearchAsync(string query, int k, CancellationToken ct);
}
