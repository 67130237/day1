using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace CreditAI.Modules.Agents;

public enum Route { SQL_ANALYST, RAG_READER, BOTH }

public sealed class IntentRouter
{
    private readonly IChatCompletionService _chat;
    private readonly string _system;

    public IntentRouter(IChatCompletionService chat, IConfiguration cfg)
    {
        _chat = chat;
        _system = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Prompts", "Router.System.txt"));
    }

    public async Task<(Route route, string notes)> RouteAsync(string text, CancellationToken ct)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(_system);
        history.AddUserMessage(text);
        var resp = await _chat.GetChatMessageContentAsync(history, cancellationToken: ct);
        var json = resp.Content ?? "{\"route\":\"RAG_READER\",\"notes\":\"fallback\"}";
        try
        {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement.GetProperty("route").GetString();
            var notes = doc.RootElement.TryGetProperty("notes", out var n) ? n.GetString() ?? "" : "";
            return (Enum.Parse<Route>(r ?? "RAG_READER"), notes);
        }
        catch { return (Route.RAG_READER, "parse-fallback"); }
    }
}
