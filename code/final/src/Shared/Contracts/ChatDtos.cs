namespace CreditAI.Shared.Contracts;

public sealed record ChatRequest(string Text, bool Stream = false, int? TopK = null);
public sealed record ChatChunk(string Type, string Data);
public sealed record ChatResponse(string Text, List<string>? Evidence = null, Dictionary<string,string>? Meta = null);

public sealed record RagIngestRequest(string Id, string Text, string? Source = null, Dictionary<string,string>? Meta = null);
public sealed record RagSearchRequest(string Query, int K = 5);
public sealed record SqlRunRequest(string Sql, int? Top = 50, int TimeoutSec = 30);
