using System.Text.Json;
using Integrations.Mcp;

namespace Integrations.Mcp;

public sealed class McpSqlToolClient : McpClientBase
{
    public McpSqlToolClient(IHttpClientFactory factory, IConfiguration cfg)
        : base(factory, cfg, "Mcp:Sql")
    {
    }

    public async Task<object> DescribeSchemaAsync(CancellationToken ct = default)
    {
        using var http = CreateClient();
        var resp = await http.PostAsync("tools/sql.describeSchema", Json(new { }), ct);
        return await ReadJsonAsync<object>(resp, ct);
    }

    public async Task<object> RunQueryRawAsync(string sql, IEnumerable<object?>? parameters = null, int? timeoutSeconds = null, CancellationToken ct = default)
    {
        using var http = CreateClient();
        var payload = new { sql, parameters = parameters?.ToArray(), timeoutSeconds };
        var resp = await http.PostAsync("tools/sql.runQuery", Json(payload), ct);
        return await ReadJsonAsync<object>(resp, ct);
    }

    public async Task<object> RunQueryStructuredAsync(string view, Dictionary<string, object?>? eq = null, string[]? columns = null, int? top = null, int? timeoutSeconds = null, CancellationToken ct = default)
    {
        using var http = CreateClient();
        var payload = new { view, eq, columns, top, timeoutSeconds };
        var resp = await http.PostAsync("tools/sql.runQuery", Json(payload), ct);
        return await ReadJsonAsync<object>(resp, ct);
    }
}
