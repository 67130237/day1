using CreditAI.Modules.Agents;
using CreditAI.Shared.Contracts;
using Route = CreditAI.Modules.Agents.Route;

namespace CreditAI.Orchestrator.Modules.Orchestration;

public sealed class Orchestrator
{
    private readonly IntentRouter _router;
    private readonly SqlAnalystAgent _sql;
    private readonly RagReaderAgent _rag;
    private readonly AnswerComposer _composer;

    public Orchestrator(IntentRouter router, SqlAnalystAgent sql, RagReaderAgent rag, AnswerComposer composer)
    {
        _router = router;
        _sql = sql;
        _rag = rag;
        _composer = composer;
    }

    public async Task<ChatResponse> ProcessAsync(ChatRequest req, CancellationToken ct)
    {
        var (route, notes) = await _router.RouteAsync(req.Text, ct);
        var drafts = new List<AgentDraft>();

        if (route == Route.BOTH)
        {
            var ragDraft = await _rag.HandleAsync(req.Text, ct);
            drafts.Add(ragDraft);

            var sqlDraft = await _sql.HandleAsync(req.Text, ct, ragDraft.Evidence);
            drafts.Add(sqlDraft);
        }
        else if (route == Route.SQL_ANALYST)
        {
            drafts.Add(await _sql.HandleAsync(req.Text, ct));
        }
        else // RAG_READER
        {
            drafts.Add(await _rag.HandleAsync(req.Text, ct));
        }

        var answer = await _composer.ComposeAsync(req.Text, drafts, ct);
        return new ChatResponse(
            answer,
            drafts.SelectMany(d => d.Evidence).ToList(),
            new Dictionary<string, string> { { "route", route.ToString() }, { "notes", notes } }
        );
    }
}
