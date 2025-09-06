using ChatApi.Shared.Abstractions;
using System.Text.Json;

namespace ChatApi.Guardrails;

public sealed class RbacGuard : IRbacGuard
{
    private readonly RbacPolicy _policy;

    public RbacGuard()
    {
        // For MVP, load embedded minimal policy; in real use, read from config or DB.
        _policy = RbacPolicy.Default();
    }

    public Task EnsureAllowedAsync(string userId, string conversationId, CancellationToken ct)
    {
        // MVP: allow all known users; block if explicitly denied
        if (_policy.DenyUsers.Contains(userId))
            throw new UnauthorizedAccessException($"User '{userId}' is denied by policy.");

        return Task.CompletedTask;
    }

    private sealed record RbacPolicy(HashSet<string> AllowUsers, HashSet<string> DenyUsers)
    {
        public static RbacPolicy Default() => new(new HashSet<string> { "*" }, new HashSet<string>());
    }
}
