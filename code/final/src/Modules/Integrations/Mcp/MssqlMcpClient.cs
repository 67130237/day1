using System.Text.Json.Nodes;

namespace CreditAI.Modules.Integrations.Mcp;

public interface IMssqlMcpClient
{
    Task<JsonNode?> ExecuteSqlAsync(string sql, int? top, int timeoutSec, CancellationToken ct);
    Task<JsonNode?> ListTablesAsync(CancellationToken ct);
    Task<JsonNode?> DescribeTableAsync(string table, CancellationToken ct);
}

public sealed class MssqlMcpClient : IMssqlMcpClient
{
    private readonly IMcpClient _client;
    private readonly string _execTool = "mssql.execute_sql";
    private readonly string _listTool = "mssql.list_tables";
    private readonly string _descTool = "mssql.describe_table";

    public MssqlMcpClient(IMcpClient client) => _client = client;

    public Task<JsonNode?> ListTablesAsync(CancellationToken ct)
        => _client.CallToolAsync(_listTool, null, ct);

    public Task<JsonNode?> DescribeTableAsync(string table, CancellationToken ct)
    {
        return _client.CallToolAsync(_descTool, JsonNode.Parse($"{{\"table\":\"{table}\"}}"), ct);
    }

    public Task<JsonNode?> ExecuteSqlAsync(string sql, int? top, int timeoutSec, CancellationToken ct)
    {
        var obj = new JsonObject
        {
            ["sql"] = sql,
            ["top"] = top is null ? null : JsonValue.Create(top.Value),
            ["timeoutSec"] = timeoutSec
        };
        return _client.CallToolAsync(_execTool, obj, ct);
    }
}
