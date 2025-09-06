using CreditAI.Modules.Rag;
using System.Text.Json.Nodes;

namespace CreditAI.Modules.Agents;

public sealed class RagReaderAgent : IAgent
{
    public string Name => "RAG_READER";
    private readonly IRagMcpClient _rag;

    public RagReaderAgent(IRagMcpClient rag) => _rag = rag;

    public async Task<AgentDraft> HandleAsync(string text, CancellationToken ct, IEnumerable<string>? context = null)
    {
        var res = await _rag.SearchAsync(text, 5, ct);
        var evid = new List<string>();
        if (res is JsonArray arr)
        {
            foreach (var n in arr) evid.Add(n?.ToJsonString() ?? "");
        }
        else if (res is not null)
        {
            evid.Add(res.ToJsonString());
        }
        return new AgentDraft
        {
            ProposedAnswer = "สรุปจากเอกสารใน evidence",
            Evidence = evid,
            Meta = new Dictionary<string, string> { ["agent"] = Name }
        };
    }
}
