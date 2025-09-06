using ChatApi.SemanticKernel;
using ChatApi.Shared.Abstractions;
using System.IO;

namespace ChatApi.Orchestrator;
public sealed class SkAnswerComposer : IAnswerComposer
{
    private readonly SkKernelFacade _sk;
    private readonly IPromptRepository _prompts;

    public SkAnswerComposer(SkKernelFacade sk, IPromptRepository prompts)
    {
        _sk = sk;
        _prompts = prompts;
    }

    public async Task<string> ComposeAsync(ComposeInput input, CancellationToken ct)
    {
        IPromptRepository _prompts;
        var system = await _prompts.GetAsync("ReAct.system.txt", ct);
        var user = BuildUserMessage(input); // รวม evidence/context จาก agents
        return await _sk.ChatCompleteAsync(system, user, modelOverride: null, ct);
    }

    public async IAsyncEnumerable<string> ComposeStreamAsync(
        ComposeInput input,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var system = await _prompts.GetAsync("ReAct.system.txt", ct);
        var user = BuildUserMessage(input);

        await foreach (var tok in _sk.ChatStreamAsync(system, user, modelOverride: null, ct))
            yield return tok;
    }

    private static string BuildUserMessage(ComposeInput x)
    {
        // ปรับโครงข้อความได้ตามสไตล์ของคุณ (ReAct/tool traces/citations)
        var ev = (x.Evidence is { Count: > 0 })
            ? string.Join("\n", x.Evidence)
            : "(no evidence)";

        return $"""
        # User
        {x.UserMessage}

        # Evidence
        {ev}
        """;
    }
}
//public sealed class SkAnswerComposer : IAnswerComposer
//{
//    private readonly ISkKernelFacade _kernel;
//    public SkAnswerComposer(ISkKernelFacade kernel) => _kernel = kernel;

//    public async Task<AgentReply> ComposeAsync(UserTurn turn, AgentReply draft, CancellationToken ct)
//    {
//        var sysPath = Path.Combine(AppContext.BaseDirectory, "ReAct.system.txt");
//        var sys = File.Exists(sysPath) ? File.ReadAllText(sysPath) : "You are a helpful assistant.";
//        var composed = await _kernel.RewriteWithContextAsync(sys, string.Empty, draft.Text, ct);
//        return draft with { Text = composed };
//    }
//}
