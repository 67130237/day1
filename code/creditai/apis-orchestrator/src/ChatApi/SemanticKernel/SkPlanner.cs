using System.Text.Json;

using ChatApi.Shared.Abstractions;

namespace ChatApi.SemanticKernel;

public sealed class SkPlanner : IPlanner
{
    private readonly SkKernelFacade _sk;
    private readonly IPromptRepository _prompts;

    public SkPlanner(SkKernelFacade sk, IPromptRepository prompts)
    {
        _sk = sk;
        _prompts = prompts;
    }

    public async Task<IReadOnlyList<PlanStep>> PlanAsync(string userMessage, CancellationToken ct)
    {
        // Prompt แผน: บังคับคืน JSON array ของ agent names
        // ตัวอย่างไฟล์: Shared.Prompts/Planner.multi.txt (ดูด้านล่าง)
        var prompt = await _prompts.GetAsync("Planner.multi.txt", ct);
        var payload = new { input = userMessage };

        // คาดหวัง JSON เช่น: {"Agents":["RAG_READER","SQL_ANALYST"],"Args":{"SQL_ANALYST":{"hint":"kpi"}}}
        var raw = await _sk.RunPromptAsync(prompt, new() { ["input"] = userMessage }, ct);

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            var steps = new List<PlanStep>();

            if (root.TryGetProperty("Agents", out var agentsEl) && agentsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var a in agentsEl.EnumerateArray())
                {
                    var name = a.GetString() ?? string.Empty;
                    Dictionary<string, string>? args = null;

                    if (root.TryGetProperty("Args", out var argsEl) &&
                        argsEl.ValueKind == JsonValueKind.Object &&
                        argsEl.TryGetProperty(name, out var argForAgent) &&
                        argForAgent.ValueKind == JsonValueKind.Object)
                    {
                        args = argForAgent.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString() ?? "");
                    }

                    if (!string.IsNullOrWhiteSpace(name))
                        steps.Add(new PlanStep(name, args));
                }
            }

            // fallback เผื่อ LLM ส่งรูปแบบไม่ตรง
            if (steps.Count == 0)
                steps.Add(new PlanStep("RAG_READER"));

            return steps;
        }
        catch
        {
            // fallback ปลอดภัย
            return new[] { new PlanStep("RAG_READER") };
        }
    }
}
