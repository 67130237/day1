using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace CreditAI.Modules.Agents;

public sealed class AnswerComposer
{
    private readonly IChatCompletionService _chat;
    private readonly string _system;

    public AnswerComposer(IChatCompletionService chat)
    {
        _chat = chat;
        _system = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Prompts", "Composer.System.txt"));
    }

    public async Task<string> ComposeAsync(string userText, IEnumerable<AgentDraft> drafts, CancellationToken ct)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(_system);
        var evid = string.Join("\n", drafts.SelectMany(d => d.Evidence).Select(e => $"- {e}"));
        var merged = $"[USER QUESTION]\n{userText}\n\n[AGENT DRAFTS]\n{string.Join("\n", drafts.Select(d => d.ProposedAnswer))}\n\n[EVIDENCE]\n{evid}";
        history.AddUserMessage(merged);
        var resp = await _chat.GetChatMessageContentAsync(history, cancellationToken: ct);
        return resp.Content ?? "(no answer)";
    }
}
