using Mcp.Sql.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SqlOptions>(builder.Configuration.GetSection("Sql"));
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SchemaProvider>();
builder.Services.AddSingleton<SqlReadOnlyValidator>();
builder.Services.AddScoped<SqlRunner>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/tools/sql.describeSchema", async (
    SchemaProvider schemaProvider,
    IOptions<SqlOptions> sqlOpt,
    CancellationToken ct) =>
{
    var sw = Stopwatch.StartNew();
    var views = await schemaProvider.GetAllowedViewsAsync(ct);
    sw.Stop();
    return Results.Ok(new
    {
        allowedSchemas = sqlOpt.Value.AllowedSchemas ?? Array.Empty<string>(),
        allowedViews = sqlOpt.Value.AllowedViews ?? Array.Empty<string>(),
        views,
        elapsedMs = sw.Elapsed.TotalMilliseconds
    });
})
.WithName("sql.describeSchema");

app.MapPost("/tools/sql.runQuery", async Task<IResult> (
    [FromBody] RunQueryRequest request,
    SqlRunner runner,
    SqlReadOnlyValidator validator,
    IOptions<SqlOptions> sqlOpt,
    CancellationToken ct) =>
{
    if (request is null) return Results.BadRequest(new { error = "Request body is required." });

    var opts = sqlOpt.Value;
    string sql;
    List<object?> parameters = [];

    if (!string.IsNullOrWhiteSpace(request.Sql))
    {
        sql = request.Sql!;
        if (request.Parameters is { Count: > 0 }) parameters.AddRange(request.Parameters);
    }
    else
    {
        if (string.IsNullOrWhiteSpace(request.View))
            return Results.BadRequest(new { error = "Provide either `sql` or `view`." });

        if (!SqlId.IsViewAllowed(request.View!, opts.AllowedSchemas, opts.AllowedViews))
            return Results.BadRequest(new { error = $"View `{request.View}` is not allowed." });

        var top = Math.Clamp(request.Top ?? opts.MaxRows, 1, opts.MaxRows);
        var cols = (request.Columns is { Length: > 0 })
            ? string.Join(", ", request.Columns.Select(SqlId.QuoteIdentifier))
            : "*";

        var where = new List<string>();
        if (request.Eq is not null)
        {
            foreach (var kv in request.Eq)
            {
                var id = SqlId.QuoteIdentifier(kv.Key);
                var p = $"@p{parameters.Count}";
                where.Add($"{id} = {p}");
                parameters.Add(kv.Value);
            }
        }
        var whereSql = where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "";
        sql = $"SELECT TOP {top} {cols} FROM {SqlId.QualifyView(request.View!)}{whereSql}";
    }

    var validation = validator.Validate(sql);
    if (!validation.IsSafe) return Results.BadRequest(new { error = "Query rejected by validator.", reasons = validation.Reasons });

    var result = await runner.ExecuteAsync(sql, parameters, request.TimeoutSeconds, ct);
    return Results.Ok(result);
})
.WithName("sql.runQuery");

app.Run();

public sealed record RunQueryRequest(
    string? Sql,
    string? View,
    Dictionary<string, object?>? Eq,
    string[]? Columns,
    int? Top,
    int? TimeoutSeconds,
    List<object?>? Parameters
);
