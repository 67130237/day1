using ChatApi.Mcp;
using ChatApi.SemanticKernel;
using ChatApi.Shared.Abstractions;

namespace ChatApi.Agents.RagReader;

public sealed class RagReaderAgent : IAgent
{
    public string Name => "RAG_READER";
    private readonly McpRagToolClient _rag;
    private readonly SkKernelFacade _kernel;

    public RagReaderAgent(McpRagToolClient rag, SkKernelFacade kernel)
    {
        _rag = rag;
        _kernel = kernel;
    }

    public async Task<AgentReply> HandleAsync(UserTurn turn, CancellationToken ct)
    {
        var passages = await _rag.HybridSearchAsync(turn.Text, 8, null, ct);
        var (answer, cits) = await _kernel.GroundedAnswerAsync(turn.Text, passages, ct);
        return new AgentReply(turn.ConversationId, Name, answer, cits);
    }
}
