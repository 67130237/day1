//using System.Text.Json;
//using System.Text.RegularExpressions;

//using ChatApi.Mcp;
//using ChatApi.SemanticKernel;
//using ChatApi.Shared.Abstractions;

//namespace ChatApi.Agents.SqlAnalyst;


//public sealed class SqlAnalystAgent : IAgent
//{
//    public string Name => "SQL_ANALYST";

//    private readonly McpSqlToolClient _sql;
//    private readonly SkKernelFacade _sk;
//    private readonly IPromptRepository _prompts;
//    private readonly ILogger<SqlAnalystAgent>? _logger;

//    // ปรับค่าตามต้องการ
//    private const int DefaultRowLimit = 200;     // บอก LLM ให้ LIMIT/TOP
//    private const int TransportRowCap = 20;      // hard cap ตอนเรียก MCP (กัน payload บวม)

//    public SqlAnalystAgent(
//        McpSqlToolClient sql,
//        SkKernelFacade sk,
//        IPromptRepository prompts,
//        ILogger<SqlAnalystAgent>? logger = null)
//    {
//        _sql = sql;
//        _sk = sk;
//        _prompts = prompts;
//        _logger = logger;
//    }

//    public async Task<AgentDraft> HandleAsync(UserTurn turn, CancellationToken ct)
//    {
//        var system = await _prompts.GetAsync("SqlAgent.Prompt.txt", ct);
//        var userMessage = BuildUserMessage(turn.Text, DefaultRowLimit);
//        var raw = await _sk.ChatCompleteAsync(system, userMessage, modelOverride: null, ct);
//        var sqlText = ExtractSql(raw).Trim();
//        if (string.IsNullOrWhiteSpace(sqlText))
//            throw new InvalidOperationException("ไม่พบ SQL ที่สร้างจากโมเดล");
//        if (LooksDangerous(sqlText))
//            throw new InvalidOperationException("ปฏิเสธคำสั่ง SQL ที่อาจเป็นอันตราย (อนุญาตเฉพาะคำสั่งอ่านข้อมูล)");

//        var result = await _sql.RunQueryRawAsync(sqlText, parameters: null, 60,  ct);
//        var json = JsonSerializer.Serialize(
//            result,
//            new JsonSerializerOptions { WriteIndented = false, PropertyNameCaseInsensitive = true });

//        var replyText =
//                $@"ผลลัพธ์จาก SQL (ตัดทอน):
//                ```sql
//                {sqlText}
//                ```
//                {json}";

//        _logger?.LogDebug("SqlAnalystAgent produced SQL: {Sql}", sqlText);
//        return new AgentDraft { ProposedAnswer = replyText, Agent = Name };
//    }
//    public sealed record AgentDraft
//    {
//        // หลักฐาน/บริบทที่ agent หาได้ (เช่น RAG passages, สรุป rows จาก SQL)
//        public List<string> Evidence { get; init; } = new();
//        // คำตอบเบื้องต้นจาก agent (ถ้ามี)
//        public string? ProposedAnswer { get; init; }
//        // meta อื่น ๆ
//        public Dictionary<string, string>? Meta { get; init; }
//    }


//    private static string BuildUserMessage(string userText, int topN)
//    {
//        return
//$@"คำถาม/งานจากผู้ใช้:
//{userText}

//ข้อกำหนด:
//- เขียนเฉพาะคำสั่ง SQL สำหรับ MS SQL Server
//- อนุญาตเฉพาะการอ่านข้อมูล (SELECT) เท่านั้น ห้าม INSERT/UPDATE/DELETE/ALTER/DROP/TRUNCATE/EXEC
//- ให้จำกัดจำนวนแถวไม่เกิน {topN} แถว (เช่น SELECT TOP({topN}) ...)
//- ห้ามใช้คำสั่งหลายตัวคั่นด้วย ';'
//- ให้ใส่คำตอบเฉพาะภายในโค้ดบล็อค ```sql ...```";
//    }

//    private static string ExtractSql(string raw)
//    {
//        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
//        var fence = Regex.Match(raw, "```sql\s*([\s\S]*?)```", RegexOptions.IgnoreCase);
//        if (fence.Success) return fence.Groups[1].Value;
//        fence = Regex.Match(raw, "```\s*([\s\S]*?)```", RegexOptions.IgnoreCase);
//        if (fence.Success) return fence.Groups[1].Value;
//        return raw;
//    }

//    private static bool LooksDangerous(string sql)
//    {
//        var s = sql.ToLowerInvariant();
//        if (s.Contains(@";") || s.Contains(";
//        ") || s.TrimEnd().EndsWith("; "))
//            return true;

//        string[] banned =
//        [
//            "insert ", "update ", "delete ", "merge ", "alter ", "drop ",
//            "truncate ", "create ", "exec ", "execute ", "grant ", "revoke ",
//            "xp_", "sp_", "bulk insert", "into ", " declare ", " set "
//        ];

//        if (!Regex.IsMatch(s.TrimStart(), @"^select"))
//            return true;

//        foreach (var b in banned)
//            if (s.Contains(b)) return true;

//        return false;
//    }
//}
