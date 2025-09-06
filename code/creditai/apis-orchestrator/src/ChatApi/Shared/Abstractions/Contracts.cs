using System.Text.Json.Serialization;

namespace ChatApi.Shared.Abstractions;







// ===== Basic DTOs =====
//public sealed record UserTurn(string UserId, string Message);

//public sealed record IntentResult
//{
//    public string Intent { get; init; } = "unknown";
//    public double Confidence { get; init; } = 0.0;
//    // เผื่ออนาคตไว้สำหรับ slot/parameters ที่ router ดึงมา
//    public Dictionary<string, string>? Slots { get; init; }
//}

//public sealed record ComposeInput
//{
//    public string UserMessage { get; init; } = string.Empty;
//    // หลักฐาน/บริบทจาก agents (RAG passages, SQL rows สรุป, ฯลฯ)
//    public List<string>? Evidence { get; init; }
//    // Metadata เฉพาะกิจ เช่น user role, locale, correlationId
//    public Dictionary<string, string>? Meta { get; init; }
//}

// ===== Abstractions (interfaces) =====
//public interface IIntentRouter
//{
//    Task<IntentResult> RouteAsync(string userMessage, CancellationToken ct);
//}

//public interface IAnswerComposer
//{
//    Task<string> ComposeAsync(ComposeInput input, CancellationToken ct);
//    IAsyncEnumerable<string> ComposeStreamAsync(ComposeInput input, CancellationToken ct = default);
//}

public interface IPromptRepository
{
    /// <summary>โหลดเนื้อหา prompt ตามชื่อไฟล์ (เช่น "ReAct.system.txt")</summary>
    Task<string> GetAsync(string name, CancellationToken ct);
}

// (optional) ยูทิลทั่วไป
public sealed record Result<T>(bool Ok, T? Value, string? Error)
{
    public static Result<T> Success(T v) => new(true, v, null);
    public static Result<T> Fail(string e) => new(false, default, e);
}





public sealed record PlanStep(string Agent /* e.g., "SQL_ANALYST","RAG_READER","FIN_CALC" */,
                              Dictionary<string, string>? Args = null);

public interface IPlanner
{
    /// <summary>คืนแผนจากข้อความผู้ใช้ เช่น ["RAG_READER","SQL_ANALYST"]</summary>
    Task<IReadOnlyList<PlanStep>> PlanAsync(string userMessage, CancellationToken ct);
}










///// <summary>
///// Standard result container to unify success/failure flows.
///// </summary>
//public readonly record struct Result<T>
//{
//    public bool Ok { get; init; }
//    public T? Value { get; init; }
//    public string? Error { get; init; }

//    [JsonConstructor]
//    public Result(bool ok, T? value, string? error)
//    {
//        Ok = ok;
//        Value = value;
//        Error = error;
//    }

//    public static Result<T> Success(T value) => new(true, value, null);
//    public static Result<T> Fail(string message) => new(false, default, message);
//}

public enum Intent
{
    Unknown = 0,
    SqlAnalysis = 1,
    RagRead = 2,
    FinancialCalc = 3,
    ChitChat = 4
}

//public sealed record UserTurn(
//    string ConversationId,
//    string UserId,
//    string Text,
//    DateTimeOffset At
//);

//public sealed record AgentReply(
//    string ConversationId,
//    string Agent,
//    string Text,
//    IReadOnlyList<Citation>? Citations = null
//);

public sealed record Citation(string Source, string? Url, string? Snippet);

public sealed record ConversationContext(
    string ConversationId,
    IReadOnlyList<string> RecentTurns,
    string? Summary
);

//public interface IIntentRouter
//{
//    Task<Intent> RouteAsync(UserTurn turn, CancellationToken ct);
//}

//public interface IAgent
//{
//    string Name { get; }
//    Task<AgentReply> HandleAsync(UserTurn turn, CancellationToken ct);
//}

//public interface IAnswerComposer
//{
//    Task<AgentReply> ComposeAsync(UserTurn turn, AgentReply draft, CancellationToken ct);
//}

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

// ===== DTOs =====
public sealed record UserTurn
{
    public string UserId { get; init; } = string.Empty;
    public string ConversationId { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}

public sealed record IntentResult
{
    public Intent EIntent { get; init; }
    public string Intent { get; init; } = "unknown";   // "sql" | "rag" | "calc" | "chitchat" ...
    public double Confidence { get; init; } = 0.0;
    public Dictionary<string, string>? Slots { get; init; }
}

public  class AgentDraft
{
    // หลักฐาน/บริบทที่ agent หาได้ (เช่น RAG passages, สรุป rows จาก SQL)
    public List<string> Evidence { get; init; } = new();
    // คำตอบเบื้องต้นจาก agent (ถ้ามี)
    public string? ProposedAnswer { get; init; }
    // meta อื่น ๆ
    public Dictionary<string, string>? Meta { get; init; }
}

public sealed record ComposeInput
{
    public string UserMessage { get; init; } = string.Empty;
    public List<string>? Evidence { get; init; }
    public Dictionary<string, string>? Meta { get; init; }
}

public sealed record AgentReply
{
    public string Text { get; init; } = string.Empty;
    public string Agent { get; init; } = string.Empty;
    public string Intent { get; init; } = "unknown";
    public Dictionary<string, string>? Meta { get; init; }
}

// ===== Abstractions =====
public interface IIntentRouter
{
    Task<IntentResult> RouteAsync(string userMessage, CancellationToken ct);
}

public interface IAnswerComposer
{
    Task<string> ComposeAsync(ComposeInput input, CancellationToken ct);
    IAsyncEnumerable<string> ComposeStreamAsync(ComposeInput input, CancellationToken ct = default);
}

public interface IAgent
{
    string Name { get; }
    Task<AgentDraft> HandleAsync(UserTurn turn, CancellationToken ct);
}
public interface IMcpClient
{
    // เรียก tool บน MCP server ตามชื่อ + args
    Task<McpResult> InvokeAsync(string tool, IDictionary<string, object> args, CancellationToken ct);
}

public sealed class McpResult
{
    public List<string>? Columns { get; init; }
    public List<Dictionary<string, object>>? Rows { get; init; }
    public string? RawJson { get; init; } // เก็บดิบสำหรับ debug/audit
}

public interface IRbacService
{
    Task EnsureAllowedAsync(string permission, CancellationToken ct);
}

public interface ISqlPolicyValidator
{
    // deny DML/multi-statement/DDL dangerous hints ฯลฯ
    void EnsureReadOnly(string sql);
    // allowlist เฉพาะ SP ที่อนุญาต
    void EnsureWhitelistedSp(string spName);
}

public interface ISkKernelFacade
{
    // ช่วยแปลง NL -> SQL และดึง args JSON ถ้าจำเป็น
    Task<string> GenerateSqlAsync(string userText, int maxTokens, CancellationToken ct);
    Task<string> ExtractJsonArgsAsync(string userText, string spName, CancellationToken ct);
}
