using System.Collections.Concurrent;

using ChatApi.Guardrails;
using ChatApi.Shared.Abstractions;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;

namespace ChatApi.Orchestrator;

public sealed class OrchestratorService
{
    private readonly IIntentRouter _router;
    private readonly IPlanner _planner;
    private readonly IEnumerable<IAgent> _agents;
    private readonly IAnswerComposer _composer;
    private readonly IPiiMasker _pii;
    private readonly IRbacGuard _rbac;
    private readonly ILogger<OrchestratorService> _log;
    private readonly TimeSpan _perAgentTimeout = TimeSpan.FromSeconds(25); // ปรับได้ตามต้องการ

    public OrchestratorService(
        IIntentRouter router,
        IPlanner planner,
        IEnumerable<IAgent> agents,
        IAnswerComposer composer,
        IPiiMasker pii,
        IRbacGuard rbac,
        ILogger<OrchestratorService> log)
    {
        _router = router;
        _planner = planner;
        _agents = agents;
        _composer = composer;
        _pii = pii;
        _rbac = rbac;
        _log = log;
    }

    public async Task<AgentReply> HandleTurnAsync(UserTurn turn, CancellationToken ct)
    {
        // 1) Guardrails
        await _rbac.EnsureAllowedAsync(turn.UserId, turn.ConversationId, ct);
        var sanitized = turn with { Text = _pii.MaskInbound(turn.Text) };

        // 2) Router (ไว้ทำ telemetry/intent meta; planner จะตัดสินใจรายเอเจนต์)
        var intent = await _router.RouteAsync(sanitized.Text, ct);

        // 3) Planner: ตัดสินใจว่าจะเรียกเอเจนต์ใดบ้าง (1..N)
        var plan = await _planner.PlanAsync(sanitized.Text, ct);
        var selected = ResolveAgents(plan);

        if (selected.Count == 0)
            selected.Add(_agents.First()); // fallback

        // 4) Fan-out ขนาน (per-agent timeout + ล้มเหลวบางตัวได้)
        var tasks = selected.Select(a => RunAgentWithBudgetAsync(a, sanitized, plan.FirstOrDefault(p => p.Agent == a.Name)?.Args, ct)).ToArray();
        var results = await Task.WhenAll(tasks);

        // 5) รวม evidence แบบ de-dup
        var evidence = results.SelectMany(r => r.Evidence).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

        // 6) Compose คำตอบสุดท้าย
        var composed = await _composer.ComposeAsync(new ComposeInput
        {
            UserMessage = sanitized.Text,
            Evidence = evidence,
            Meta = new Dictionary<string, string>
            {
                ["intent"] = intent.Intent,
                ["agents"] = string.Join(",", selected.Select(x => x.Name))
            }
        }, ct);

        // 7) Outbound mask
        var masked = _pii.MaskOutbound(composed);
        return new AgentReply
        {
            Text = masked,
            Agent = string.Join("+", selected.Select(a => a.Name)),
            Intent = intent.Intent,
            Meta = new Dictionary<string, string>
            {
                ["called"] = string.Join(",", selected.Select(a => a.Name)),
                ["evidence_count"] = evidence.Count.ToString()
            }
        };
    }

    private List<IAgent> ResolveAgents(IReadOnlyList<PlanStep> plan)
    {
        var list = new List<IAgent>();
        foreach (var step in plan)
        {
            var a = _agents.FirstOrDefault(x => string.Equals(x.Name, step.Agent, StringComparison.OrdinalIgnoreCase));
            if (a != null && !list.Contains(a))
                list.Add(a);
        }
        return list;
    }

    private async Task<AgentDraft> RunAgentWithBudgetAsync(IAgent agent, UserTurn turn, Dictionary<string, string>? args, CancellationToken outer)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(outer);
        cts.CancelAfter(_perAgentTimeout);
        try
        {
            // ถ้าต้องการส่ง args ให้ agent: ขยายสัญญา IAgent หรือยัดไปใน turn.Meta ก็ได้
            return await agent.HandleAsync(turn, cts.Token);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Agent {Agent} failed", agent.Name);
            return new AgentDraft
            {
                Evidence = new() { $"[agent:{agent.Name}] unavailable / timed out" },
                Meta = new() { ["agent_error"] = ex.GetType().Name }
            };
        }
    }
}
