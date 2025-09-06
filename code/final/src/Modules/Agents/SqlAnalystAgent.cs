using CreditAI.Modules.Integrations.Mcp;
using Microsoft.SemanticKernel.ChatCompletion;

namespace CreditAI.Modules.Agents;

public sealed class SqlAnalystAgent : IAgent
{
    public string Name => "SQL_ANALYST";
    private readonly IMssqlMcpClient _mssql;
    private readonly IChatCompletionService _chat;
    private readonly string _system;
    private readonly int _evLength = 15;
    private readonly int _topRow = 15;
    private readonly int _timeOut = 15;

    public SqlAnalystAgent(IMssqlMcpClient mssql, IChatCompletionService chat)
    {
        _mssql = mssql;
        _chat = chat;
        _system = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Prompts", "SqlAgent.System.txt"));
    }
    public async Task<AgentDraft> HandleAsync(string text, CancellationToken ct, IEnumerable<string>? context = null)
    {
        // ถ้า user ใส่ SQL ตรงๆ
        if (text.Contains("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            var res0 = await _mssql.ExecuteSqlAsync(text, top: 50, timeoutSec: 30, ct);
            return new AgentDraft
            {
                ProposedAnswer = "ผลลัพธ์จาก SQL (ผู้ใช้ระบุมาเอง)",
                Evidence = new() { res0?.ToJsonString() ?? "(no result)" },
                Meta = new() { ["agent"] = Name, ["mode"] = "raw-sql" }
            };
        }

        // สร้าง SQL จาก Evidence
        var history = new ChatHistory();
        history.AddSystemMessage(_system);

        var evid = (context is null || !context.Any())
            ? "(no evidence)"
            : string.Join("\n- ", context.Take(_evLength));

        history.AddUserMessage($"[USER QUESTION]\n{text}\n\n[EVIDENCE]\n- {evid}");
        var sqlMsg = await _chat.GetChatMessageContentAsync(history, cancellationToken: ct);
        var sql = (sqlMsg.Content ?? "SELECT 1").Trim();

        var res = await _mssql.ExecuteSqlAsync(sql, top: _topRow, timeoutSec: _timeOut, ct);

        return new AgentDraft
        {
            ProposedAnswer = "สรุปผลจากคำถาม โดยใช้ SQL ที่สร้างจากเอกสารอ้างอิง",
            Evidence = new()
            {
                $"-- SQL GENERATED\n{sql}",
                res?.ToJsonString() ?? "(no result)"
            },
            Meta = new() { ["agent"] = Name, ["mode"] = "llm-sql" }
        };
    }

}
