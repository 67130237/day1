using static CreditAI.Modules.Integrations.Mcp.McpClientOptions;

namespace CreditAI.Modules.Integrations.Mcp;

public sealed record McpClientOptions(
    string Transport,
    string? Command,
    string[]? Args,
    Dictionary<string,string>? Env,
    HttpOptions? Http)
{
    public sealed record HttpOptions(string BaseUrl);
}
