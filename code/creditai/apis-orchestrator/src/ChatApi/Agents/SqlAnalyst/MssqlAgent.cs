using System.Text;
using System.Text.RegularExpressions;

using ChatApi.Shared.Abstractions;

namespace ChatApi.Agents.SqlAnalyst;

public sealed class MssqlAgent : IAgent
{
    public string Name => "MSSQL_AGENT";

    private readonly IMcpClient _mcp;                      // generic MCP client
    private readonly IRbacService _rbac;                   // role/permission gate
    private readonly ISqlPolicyValidator _policy;          // readonly/allowlist rules
    private readonly MssqlAgentOptions _opts;              // tool names, row caps, etc.
    private readonly ISkKernelFacade _sk;                  // optional: for NL->SQL

    public MssqlAgent(
        IMcpClient mcp,
        IRbacService rbac,
        ISqlPolicyValidator policy,
        MssqlAgentOptions opts,
        ISkKernelFacade sk)
    {
        _mcp = mcp;
        _rbac = rbac;
        _policy = policy;
        _opts = opts;
        _sk = sk;
    }

    // NOTE: original interface had `stryin text` (typo). I assume it's `string text`.
    public async Task<AgentDraft> HandleAsync(UserTurn turn, CancellationToken ct)
    {
        // 1) Guardrails: role must be allowed to query DB
        await _rbac.EnsureAllowedAsync("db.read", ct);

        var draft = new AgentDraft();

        // 2) Fast-path heuristics to pick tool
        var mentionsSp = Regex.Match(turn.Text, @"\b(sp_[A-Za-z0-9_]+)\b", RegexOptions.IgnoreCase);
        var looksLikeSelect = Regex.IsMatch(turn.Text, @"\bselect\b.+\bfrom\b", RegexOptions.IgnoreCase);

        if (mentionsSp.Success)
        {
            // choose exec_sp
            var spName = mentionsSp.Groups[1].Value;
            _policy.EnsureWhitelistedSp(spName);

            var (args, parseNote) = TryExtractArgs(turn.Text);
            if (args.Count == 0)
            {
                // fallback: ask LLM to infer args JSON schema-ish
                var argJson = await _sk.ExtractJsonArgsAsync(turn.Text, spName, ct);
                //args = JsonToDictionarySafe(argJson);
                parseNote += " | args via LLM";
            }

            // 3) Invoke MCP exec_sp tool
            var result = await CallMcpAsync(
                _opts.ExecSpTool,
                new Dictionary<string, object> { ["name"] = spName, ["args"] = args },
                ct);

            draft.Evidence.Add($"exec_sp: {spName} args={SerializeArgs(args)}");
            if (!string.IsNullOrWhiteSpace(parseNote)) draft.Evidence.Add(parseNote);

            //draft.ProposedAnswer = SummarizeResult(result, maxRows: _opts.MaxRowsForSummary);
            //draft.Meta = new Dictionary<string, string>
            //{
            //    ["tool"] = _opts.ExecSpTool,
            //    ["rows"] = GetRowCount(result).ToString()
            //};
            return draft;
        }

        // If user pasted a SELECT or asked data in natural language
        if (looksLikeSelect)
        {
            // 4a) Raw SELECT path (user provided SQL)
            var sql = ExtractSqlLiteral(turn.Text);
            _policy.EnsureReadOnly(sql);
            sql = AddTopLimitIfMissing(sql, _opts.SelectTopCap);

            var result = await CallMcpAsync(
                _opts.SelectTool,
                new Dictionary<string, object> { ["sql"] = sql, ["top"] = _opts.SelectTopCap },
                ct);

            draft.Evidence.Add($"select(sql): {TrimForLog(sql)}");
            //draft.ProposedAnswer = SummarizeResult(result, maxRows: _opts.MaxRowsForSummary);
            //draft.Meta = new Dictionary<string, string>
            //{
            //    ["tool"] = _opts.SelectTool,
            //    ["rows"] = GetRowCount(result).ToString()
            //};
            return draft;
        }
        else
        {
            // 4b) NL->SQL via SK, then SELECT
            var sql = await _sk.GenerateSqlAsync(turn.Text, maxTokens: 200, ct);
            _policy.EnsureReadOnly(sql);
            sql = AddTopLimitIfMissing(sql, _opts.SelectTopCap);

            var result = await CallMcpAsync(
                _opts.SelectTool,
                new Dictionary<string, object> { ["sql"] = sql, ["top"] = _opts.SelectTopCap },
                ct);

            draft.Evidence.Add("sql_generated_from_natural_language");
            draft.Evidence.Add($"select(sql): {TrimForLog(sql)}");
            //draft.ProposedAnswer = SummarizeResult(result, maxRows: _opts.MaxRowsForSummary);
            //draft.Meta = new Dictionary<string, string>
            //{
            //    ["tool"] = _opts.SelectTool,
            //    ["rows"] = GetRowCount(result).ToString()
            //};
            return draft;
        }
    }

    // ---- MCP call helper ----------------------------------------------------

    private async Task<McpResult> CallMcpAsync(string toolName, Dictionary<string, object> args, CancellationToken ct)
    {
        // Optional: per-call RBAC filter by tool
        await _rbac.EnsureAllowedAsync($"mcp.tool:{toolName}", ct);

        // Generic MCP invocation (tool + args) => McpResult (rows/columns/raw)
        var res = await _mcp.InvokeAsync(toolName, args, ct);
        return res;
    }

    // ---- Utilities ----------------------------------------------------------

    private static string AddTopLimitIfMissing(string sql, int top)
    {
        // naive, safe cap: if SELECT without TOP, inject TOP
        var m = Regex.Match(sql, @"^\s*select\s+top\s+\d+", RegexOptions.IgnoreCase);
        if (m.Success) return sql;
        return Regex.Replace(sql, @"^\s*select\s", $"SELECT TOP {top} ", RegexOptions.IgnoreCase);
    }

    private static string ExtractSqlLiteral(string text)
    {
        // crude extraction; in production consider code fence parsing / LLM assist
        var m = Regex.Match(text, @"(select\b.+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return m.Success ? m.Groups[1].Value.Trim() : text;
    }

    private static (Dictionary<string, object> args, string note) TryExtractArgs(string text)
    {
        // Support simple "param=value" pairs in the message (e.g., contractNo=2507...)
        var args = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var note = new StringBuilder();

        foreach (Match m in Regex.Matches(text, @"([A-Za-z0-9_]+)\s*=\s*([A-Za-z0-9_\-\.]+)"))
        {
            args[m.Groups[1].Value] = m.Groups[2].Value;
        }

        if (args.Count > 0) note.Append("args parsed from plain text");
        return (args, note.ToString());
    }

    private static int GetRowCount(McpResult result)
        => result?.Rows?.Count ?? 0;

    private static string? SummarizeResult(McpResult result, int maxRows)
    {
        if (result == null) return "No result";
        var rows = result.Rows ?? new List<Dictionary<string, object>>();
        var cols = result.Columns ?? new List<string>();

        var sb = new StringBuilder();
        if (rows.Count == 0)
        {
            sb.Append("ไม่พบข้อมูล");
        }
        else
        {
            sb.AppendLine($"ผลลัพธ์ {Math.Min(rows.Count, maxRows)} แถวแรก (จาก {rows.Count}):");
            var take = rows.Take(maxRows).ToList();
            foreach (var r in take)
            {
                var parts = cols.Select(c => $"{c}={SafeToString(r.TryGetValue(c, out var v) ? v : null)}");
                sb.AppendLine("- " + string.Join(", ", parts));
            }
        }
        return sb.ToString().TrimEnd();
    }

    private static string SerializeArgs(Dictionary<string, object> args)
        => string.Join(", ", args.Select(kv => $"{kv.Key}={kv.Value}"));

    private static string SafeToString(object? v) => v?.ToString() ?? "null";

    private static string TrimForLog(string s)
        => s.Length <= 600 ? s : s.Substring(0, 600) + "...(trimmed)";

}

public sealed class MssqlAgentOptions
{
    // ตั้งชื่อ tool ให้ตรงกับที่ MSSQL MCP server register ไว้
    // (repo ระบุฟีเจอร์หลักคือ list tables และ execute SQL — ชื่อจริงอาจเป็นเช่น "mssql.execute_sql" หรือ "sql.query")
    // ปรับให้ตรงกับ server ของคุณ
    public string SelectTool { get; init; } = "mssql.execute_sql"; // read-only SELECT
    public string ExecSpTool { get; init; } = "mssql.exec_sp";    // whitelisted SP

    // caps & presentation
    public int SelectTopCap { get; init; } = 200;          // inject TOP if missing
    public int MaxRowsForSummary { get; init; } = 20;      // evidence rendering
}
