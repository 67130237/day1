using System.Text.Json.Serialization;

namespace Shared.Abstractions;

/// <summary>
/// Standard result container to unify success/failure flows.
/// </summary>
public readonly record struct Result<T>
{
    public bool Ok { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }

    [JsonConstructor]
    public Result(bool ok, T? value, string? error)
    {
        Ok = ok;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Fail(string message) => new(false, default, message);
}

public enum Intent
{
    Unknown = 0,
    SqlAnalysis = 1,
    RagRead = 2,
    FinancialCalc = 3,
    ChitChat = 4
}

public sealed record UserTurn(
    string ConversationId,
    string UserId,
    string Text,
    DateTimeOffset At
);

public sealed record AgentReply(
    string ConversationId,
    string Agent,
    string Text,
    IReadOnlyList<Citation>? Citations = null
);

public sealed record Citation(string Source, string? Url, string? Snippet);

public sealed record ConversationContext(
    string ConversationId,
    IReadOnlyList<string> RecentTurns,
    string? Summary
);

public interface IIntentRouter
{
    Task<Intent> RouteAsync(UserTurn turn, CancellationToken ct);
}

public interface IAgent
{
    string Name { get; }
    Task<AgentReply> HandleAsync(UserTurn turn, CancellationToken ct);
}

public interface IAnswerComposer
{
    Task<AgentReply> ComposeAsync(UserTurn turn, AgentReply draft, CancellationToken ct);
}

/// <summary>
/// Mask PII on inbound/outbound messages (simple interface so UI & agents can share one implementation).
/// </summary>
public interface IPiiMasker
{
    string MaskInbound(string text);
    string MaskOutbound(string text);
}

/// <summary>
/// Simple RBAC guard to check whether a user can interact with a conversation or resource.

/// </summary>
public interface IRbacGuard
{
    Task EnsureAllowedAsync(string userId, string conversationId, CancellationToken ct);
}
