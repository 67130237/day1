using Shared.Abstractions;
using Guardrails.Core;

namespace Orchestrator.Core;

public sealed class OrchestratorService
{
    private readonly IIntentRouter _router;
    private readonly IEnumerable<IAgent> _agents;
    private readonly IAnswerComposer _composer;
    private readonly IPiiMasker _pii;
    private readonly IRbacGuard _rbac;

    public OrchestratorService(
        IIntentRouter router,
        IEnumerable<IAgent> agents,
        IAnswerComposer composer,
        IPiiMasker pii,
        IRbacGuard rbac)
    {
        _router = router;
        _agents = agents;
        _composer = composer;
        _pii = pii;
        _rbac = rbac;
    }

    public async Task<AgentReply> HandleTurnAsync(UserTurn turn, CancellationToken ct)
    {
        await _rbac.EnsureAllowedAsync(turn.UserId, turn.ConversationId, ct);
        var sanitized = turn with { Text = _pii.MaskInbound(turn.Text) };

        var intent = await _router.RouteAsync(sanitized, ct);
        var agent = SelectAgent(intent) ?? _agents.First();

        var draft = await agent.HandleAsync(sanitized, ct);
        var final = await _composer.ComposeAsync(sanitized, draft, ct);
        var masked = final with { Text = _pii.MaskOutbound(final.Text) };
        return masked;
    }

    private IAgent? SelectAgent(Intent intent) => intent switch
    {
        Intent.SqlAnalysis => _agents.FirstOrDefault(a => a.Name == "SQL_ANALYST"),
        Intent.RagRead => _agents.FirstOrDefault(a => a.Name == "RAG_READER"),
        Intent.FinancialCalc => _agents.FirstOrDefault(a => a.Name == "FIN_CALC"),
        Intent.ChitChat => _agents.FirstOrDefault(a => a.Name == "RAG_READER"),
        _ => null
    };
}
