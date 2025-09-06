//using System.Text.Json;
//using System.Text.Json.Serialization;
//using ChatApi.Shared.Abstractions;
//using Integrations.OpenRouter;

//namespace ChatApi.SemanticKernel;

///// <summary>
///// Minimal facade that wraps LLM calls for: intent classification, SQL generation, RAG answer, calculation extraction/explanation.
///// Loads prompt templates from Shared.Prompts output directory (copy-to-output).
///// </summary>
//public interface ISkKernelFacadeManual
//{
//    Task<string> ClassifyIntentAsync(string text, string[] labels, CancellationToken ct);
//    Task<string> GenerateSqlAsync(string question, int top = 100, CancellationToken ct = default);
//    Task<(string Clean, IReadOnlyList<Citation> Citations)> GroundedAnswerAsync(string question, IReadOnlyList<string> passages, CancellationToken ct = default);
//    Task<string> RewriteWithContextAsync(string systemPrompt, string context, string draft, CancellationToken ct = default);
//    Task<string> ExtractCalcParamsJsonAsync(string text, CancellationToken ct = default);
//    Task<string> ExplainCalcAsync(object result, CancellationToken ct = default);
//}

//public sealed class SkKernelFacadeManual : ISkKernelFacadeManual
//{
//    private readonly OpenRouterClient _client;

//    public SkKernelFacadeManual(OpenRouterClient client)
//    {
//        _client = client;
//    }

//    private static string ReadPrompt(string name)
//    {
//        var path = Path.Combine(AppContext.BaseDirectory, name);
//        if (File.Exists(path)) return File.ReadAllText(path);

//        // Fallback to prompts folder under Shared.Prompts copied structure
//        var alt = Path.Combine(AppContext.BaseDirectory, "Shared.Prompts", name);
//        if (File.Exists(alt)) return File.ReadAllText(alt);

//        throw new FileNotFoundException($"Prompt '{name}' not found in output.");
//    }

//    public async Task<string> ClassifyIntentAsync(string text, string[] labels, CancellationToken ct)
//    {
//        var sys = ReadPrompt("IntentRouter.classify.txt");
//        var user = $@"{text}
//                    Labels: {string.Join(", ", labels)}
//                    Respond with exactly one label.";
//        var resp = await _client.ChatAsync(sys, user, ct);
//        return resp.Trim();
//    }

//    public async Task<string> GenerateSqlAsync(string question, int top = 100, CancellationToken ct = default)
//    {
//        var sys = ReadPrompt("Sql.generate.txt").Replace("{top}", top.ToString());
//        var resp = await _client.ChatAsync(sys, question, ct);
//        return resp.Trim();
//    }

//    public async Task<(string Clean, IReadOnlyList<Citation> Citations)> GroundedAnswerAsync(string question, IReadOnlyList<string> passages, CancellationToken ct = default)
//    {
//        var sys = ReadPrompt("Rag.answer.txt");
//        var refs = new List<string>();
//        for (int i = 0; i < passages.Count; i++)
//        {
//            refs.Add($"[{i+1}] {passages[i]}");
//        }
//        var user = $@"{question}
//References: {string.Join("", refs)}";
//        var resp = await _client.ChatAsync(sys, user, ct);
//        // Very light-weight extraction of [#] patterns to citations; map back to passages order.
//        var cits = new List<Citation>();
//        for (int i = 0; i < passages.Count; i++)
//        {
//            var tag = $"[{i+1}]";
//            if (resp.Contains(tag, StringComparison.Ordinal))
//                cits.Add(new Citation($"ref-{i+1}", null, passages[i].Length > 160 ? passages[i][..160] + "…" : passages[i]));
//        }
//        return (resp.Trim(), cits);
//    }

//    public async Task<string> RewriteWithContextAsync(string systemPrompt, string context, string draft, CancellationToken ct = default)
//    {
//        var user = $@"Context:
//{context}

//Draft:
//{draft}

//Task: Improve the draft using the context. Keep it concise and correct.";
//        var resp = await _client.ChatAsync(systemPrompt, user, ct);
//        return resp.Trim();
//    }

//    public async Task<string> ExtractCalcParamsJsonAsync(string text, CancellationToken ct = default)
//    {
//        var sys = ReadPrompt("Calc.extract.txt");
//        var json = await _client.ChatAsync(sys, text, ct);
//        return json.Trim();
//    }

//    public async Task<string> ExplainCalcAsync(object result, CancellationToken ct = default)
//    {
//        var sys = ReadPrompt("Calc.explain.txt");
//        var payload = JsonSerializer.Serialize(result);
//        var user = $@"Result JSON:
//{payload}

//Explain:";
//        var resp = await _client.ChatAsync(sys, user, ct);
//        return resp.Trim();
//    }
//}
