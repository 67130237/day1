using Integrations.Mcp;
using Integrations.SemanticKernel;
using Shared.Abstractions;

namespace Agents.SqlAnalyst;

public sealed class SqlAnalystAgent : IAgent
{
    public string Name => "SQL_ANALYST";
    private readonly McpSqlToolClient _sql;
    private readonly ISkKernelFacade _kernel;

    public SqlAnalystAgent(McpSqlToolClient sql, ISkKernelFacade kernel)
    {
        _sql = sql;
        _kernel = kernel;
    }

    public async Task<AgentReply> HandleAsync(UserTurn turn, CancellationToken ct)
    {
        var sql = await _kernel.GenerateSqlAsync(turn.Text, 200, ct);
        var result = await _sql.RunQueryRawAsync(sql, null, 20, ct);
        // Keep it simple: just serialize result summary
        var text = $"ผลลัพธ์จาก SQL (ตัดทอน):\n```sql\n{sql}\n```\n{System.Text.Json.JsonSerializer.Serialize(result)}";
        return new AgentReply(turn.ConversationId, Name, text);
    }
}
