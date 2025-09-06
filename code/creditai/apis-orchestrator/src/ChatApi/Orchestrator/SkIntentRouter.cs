using ChatApi.SemanticKernel;
using ChatApi.Shared.Abstractions;

using Microsoft.AspNetCore.Mvc;

namespace ChatApi.Orchestrator;

public sealed class SkIntentRouter : IIntentRouter
{
    private readonly SkKernelFacade _sk;
    private readonly IPromptRepository _prompts; // คุณมี Shared.Prompts อยู่แล้ว

    public SkIntentRouter(SkKernelFacade sk, IPromptRepository prompts)
    {
        _sk = sk;
        _prompts = prompts;
    }

    public async Task<IntentResult> RouteAsync(string userMessage, CancellationToken ct)
    {

        var prompt = await _prompts.GetAsync("IntentRouter.classify.txt", ct);
        var payload = new { input = userMessage };

        // ให้ LLM คืนเป็น JSON {"Intent": "SqlAnalysis", "Confidence": 0.9}
        var result = await _sk.RunJsonAsync<IntentResult>(prompt, payload, ct);

        if (result is null || string.IsNullOrWhiteSpace(result.Intent))
        {
            return new IntentResult
            {
                Intent = Intent.ChitChat.ToString(),
                Confidence = 0.3
            };
        }

        // map string -> enum แบบ safe
        if (!Enum.TryParse<Intent>(result.Intent, ignoreCase: true, out var parsed))
        {
            parsed = Intent.Unknown;
        }

        // คืน IntentResult ที่ Intent เป็นชื่อ enum ชัดเจน
        return new IntentResult
        {
            EIntent = parsed,
            Intent = parsed.ToString(),
            Confidence = result.Confidence,
            Slots = result.Slots
        };
    }
}
//public sealed class SkIntentRouter : IIntentRouter
//{
//    private readonly ISkKernelFacade _kernel;
//    public SkIntentRouter(ISkKernelFacade kernel) => _kernel = kernel;

//    public async Task<Intent> RouteAsync(UserTurn turn, CancellationToken ct)
//    {
//        var label = await _kernel.ClassifyIntentAsync(turn.Text,
//            [
//                "SqlAnalysis",
//                "RagRead",
//                "FinancialCalc",
//                "ChitChat"
//            ], ct);
//        return label switch
//        {
//            "SqlAnalysis" => Intent.SqlAnalysis,
//            "RagRead" => Intent.RagRead,
//            "FinancialCalc" => Intent.FinancialCalc,
//            "ChitChat" => Intent.ChitChat,
//            _ => Intent.Unknown
//        };
//    }
//}
