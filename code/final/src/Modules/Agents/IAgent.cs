namespace CreditAI.Modules.Agents;

public interface IAgent
{
    string Name { get; }
    Task<AgentDraft> HandleAsync(string text, CancellationToken ct, IEnumerable<string>? context = null);
}

public sealed class AgentDraft
{
    public List<string> Evidence { get; init; } = new();
    public string? ProposedAnswer { get; init; }
    public Dictionary<string,string>? Meta { get; init; }
}
