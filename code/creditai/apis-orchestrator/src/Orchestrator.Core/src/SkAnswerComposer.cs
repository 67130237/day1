using Integrations.SemanticKernel;
using Shared.Abstractions;
using System.IO;

namespace Orchestrator.Core;

public sealed class SkAnswerComposer : IAnswerComposer
{
    private readonly ISkKernelFacade _kernel;
    public SkAnswerComposer(ISkKernelFacade kernel) => _kernel = kernel;

    public async Task<AgentReply> ComposeAsync(UserTurn turn, AgentReply draft, CancellationToken ct)
    {
        var sysPath = Path.Combine(AppContext.BaseDirectory, "ReAct.system.txt");
        var sys = File.Exists(sysPath) ? File.ReadAllText(sysPath) : "You are a helpful assistant.";
        var composed = await _kernel.RewriteWithContextAsync(sys, string.Empty, draft.Text, ct);
        return draft with { Text = composed };
    }
}
