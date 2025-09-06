using Integrations.SemanticKernel;
using Shared.Abstractions;

namespace Orchestrator.Core;

public sealed class SkIntentRouter : IIntentRouter
{
    private readonly ISkKernelFacade _kernel;
    public SkIntentRouter(ISkKernelFacade kernel) => _kernel = kernel;

    public async Task<Intent> RouteAsync(UserTurn turn, CancellationToken ct)
    {
        var label = await _kernel.ClassifyIntentAsync(turn.Text, new[] { "SqlAnalysis", "RagRead", "FinancialCalc", "ChitChat" }, ct);
        return label switch
        {
            "SqlAnalysis" => Intent.SqlAnalysis,
            "RagRead" => Intent.RagRead,
            "FinancialCalc" => Intent.FinancialCalc,
            "ChitChat" => Intent.ChitChat,
            _ => Intent.Unknown
        };
    }
}
