using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Data;

namespace Mcp.Sql.Core;

public sealed class SchemaProvider
{
    private readonly string _connString;
    private readonly SqlOptions _opts;
    private readonly IMemoryCache _cache;

    public SchemaProvider(IConfiguration cfg, IOptions<SqlOptions> opts, IMemoryCache cache)
    {
        _connString = cfg.GetConnectionString("SqlRo") ?? throw new InvalidOperationException("ConnectionStrings:SqlRo is required.");
        _opts = opts.Value;
        _cache = cache;
    }

    public async Task<IReadOnlyList<ViewSchema>> GetAllowedViewsAsync(CancellationToken ct)
    {
        return await _cache.GetOrCreateAsync("schema:views", async e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
            using var conn = new SqlConnection(_connString);
            await conn.OpenAsync(ct);

            var sql = @"
SELECT v.TABLE_SCHEMA, v.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE, c.IS_NULLABLE
FROM INFORMATION_SCHEMA.VIEWS v
JOIN INFORMATION_SCHEMA.COLUMNS c
  ON v.TABLE_SCHEMA = c.TABLE_SCHEMA AND v.TABLE_NAME = c.TABLE_NAME
ORDER BY v.TABLE_SCHEMA, v.TABLE_NAME, c.ORDINAL_POSITION;";
            using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text, CommandTimeout = 10 };
            using var reader = await cmd.ExecuteReaderAsync(ct);

            var map = new Dictionary<(string S, string N), ViewSchema>(StringComparer.OrdinalIgnoreCase);
            while (await reader.ReadAsync(ct))
            {
                var s = reader.GetString(0);
                var n = reader.GetString(1);

                if (!IsSchemaAllowed(s)) continue;
                if (!IsViewAllowed(s, n)) continue;

                var key = (s, n);
                if (!map.TryGetValue(key, out var v))
                {
                    v = new ViewSchema(s, n, new List<ColumnSchema>());
                    map[key] = v;
                }
                ((List<ColumnSchema>)v.Columns).Add(new ColumnSchema(reader.GetString(2), reader.GetString(3), reader.GetString(4) == "YES"));
            }
            return map.Values.OrderBy(x => x.Schema).ThenBy(x => x.Name).ToList();
        }) ?? Array.Empty<ViewSchema>();
    }

    private bool IsSchemaAllowed(string schema)
    {
        var a = _opts.AllowedSchemas ?? Array.Empty<string>();
        if (a.Length == 0) return true;
        return a.Contains(schema, StringComparer.OrdinalIgnoreCase);
    }
    private bool IsViewAllowed(string schema, string name)
    {
        var a = _opts.AllowedViews ?? Array.Empty<string>();
        if (a.Length == 0) return true;
        var fully = $"{schema}.{name}";
        return a.Contains(fully, StringComparer.OrdinalIgnoreCase);
    }
}

public sealed record ViewSchema(string Schema, string Name, IReadOnlyList<ColumnSchema> Columns);
public sealed record ColumnSchema(string Name, string SqlType, bool IsNullable);

public sealed class SqlReadOnlyValidator
{
    private readonly Regex[] _rejects;
    private readonly SqlOptions _opts;
    public SqlReadOnlyValidator(IConfiguration cfg, IOptions<SqlOptions> opts)
    {
        _opts = opts.Value;
        var patterns = cfg.GetSection("Security:RejectPatterns").Get<string[]>() ?? Array.Empty<string>();
        _rejects = patterns.Select(p => new Regex(p, RegexOptions.Compiled)).ToArray();
    }

    public ValidationResult Validate(string sql)
    {
        var reasons = new List<string>();
        if (string.IsNullOrWhiteSpace(sql)) { reasons.Add("Empty SQL."); return ValidationResult.Fail(reasons); }
        var norm = sql.Trim();
        var startsOk = Regex.IsMatch(norm, @"(?is)^\s*(WITH\s+[\s\S]+?\)\s*SELECT|SELECT)\b");
        if (!startsOk) reasons.Add("Only SELECT or WITH ... SELECT is allowed.");
        foreach (var r in _rejects) if (r.IsMatch(norm)) reasons.Add($"Pattern rejected: {r}");
        var ids = Regex.Matches(norm, @"(?i)\b([A-Z_][A-Z0-9_]*)\.([A-Z_][A-Z0-9_]*)\b", RegexOptions.IgnoreCase);
        foreach (Match m in ids)
        {
            var schema = m.Groups[1].Value; var name = m.Groups[2].Value;
            if (!SqlId.SchemaAllowed(schema, _opts.AllowedSchemas)) reasons.Add($"Schema `{schema}` is not allowed.");
            if (!SqlId.ViewAllowed(schema, name, _opts.AllowedViews)) reasons.Add($"View `{schema}.{name}` is not allowed.");
        }
        return reasons.Count == 0 ? ValidationResult.Ok() : ValidationResult.Fail(reasons);
    }
}

public sealed record ValidationResult(bool IsSafe, IReadOnlyList<string> Reasons)
{
    public static ValidationResult Ok() => new(true, Array.Empty<string>());
    public static ValidationResult Fail(IReadOnlyList<string> r) => new(false, r);
}

public static class SqlId
{
    public static string QuoteIdentifier(string id) => $"[{id.Replace("]", "]]")}]";
    public static string QualifyView(string input)
    {
        var parts = input.Split('.', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 ? $"{QuoteIdentifier(parts[0])}.{QuoteIdentifier(parts[1])}" : $"{QuoteIdentifier("dbo")}.{QuoteIdentifier(parts[0])}";
    }
    public static bool SchemaAllowed(string schema, string[]? allowedSchemas)
        => allowedSchemas is null || allowedSchemas.Length == 0 || allowedSchemas.Contains(schema, StringComparer.OrdinalIgnoreCase);
    public static bool ViewAllowed(string schema, string name, string[]? allowedViews)
        => allowedViews is null || allowedViews.Length == 0 || allowedViews.Contains($"{schema}.{name}", StringComparer.OrdinalIgnoreCase);
    public static bool IsViewAllowed(string view, string[]? allowedSchemas, string[]? allowedViews)
    {
        var parts = view.Split('.', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var schema = parts.Length == 2 ? parts[0] : "dbo";
        var name = parts.Length == 2 ? parts[1] : parts[0];
        return SchemaAllowed(schema, allowedSchemas) && ViewAllowed(schema, name, allowedViews);
    }
}
