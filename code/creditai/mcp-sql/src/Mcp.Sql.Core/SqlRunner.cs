using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics;

namespace Mcp.Sql.Core;

public sealed class SqlRunner
{
    private readonly string _connString;
    private readonly SqlOptions _opts;

    public SqlRunner(IConfiguration cfg, IOptions<SqlOptions> options)
    {
        _connString = cfg.GetConnectionString("SqlRo") ?? throw new InvalidOperationException("ConnectionStrings:SqlRo is required.");
        _opts = options.Value;
    }

    public async Task<object> ExecuteAsync(string sql, List<object?> parameters, int? timeoutSeconds, CancellationToken ct)
    {
        using var conn = new SqlConnection(_connString);
        await conn.OpenAsync(ct);

        using var cmd = new SqlCommand($"SET NOCOUNT ON; {sql}", conn)
        {
            CommandType = CommandType.Text,
            CommandTimeout = Math.Max(1, timeoutSeconds ?? _opts.DefaultCommandTimeoutSeconds)
        };

        for (var i = 0; i < parameters.Count; i++)
        {
            cmd.Parameters.AddWithValue($"@p{i}", parameters[i] ?? DBNull.Value);
        }

        var sw = Stopwatch.StartNew();
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);

        var columns = new List<string>();
        var types = new List<string>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            columns.Add(reader.GetName(i));
            types.Add(reader.GetDataTypeName(i));
        }

        var rows = new List<object?[]>();
        int count = 0;
        while (await reader.ReadAsync(ct))
        {
            var arr = new object?[reader.FieldCount];
            reader.GetValues(arr);
            rows.Add(arr);
            count++;
            if (count >= _opts.MaxRows) break;
        }
        sw.Stop();

        return new { columns, columnTypes = types, rows, rowCount = rows.Count, truncated = count >= _opts.MaxRows, elapsedMs = sw.Elapsed.TotalMilliseconds };
    }
}

public sealed class SqlOptions
{
    public string[]? AllowedSchemas { get; set; }
    public string[]? AllowedViews { get; set; }
    public int MaxRows { get; set; } = 10000;
    public int DefaultCommandTimeoutSeconds { get; set; } = 15;
}
