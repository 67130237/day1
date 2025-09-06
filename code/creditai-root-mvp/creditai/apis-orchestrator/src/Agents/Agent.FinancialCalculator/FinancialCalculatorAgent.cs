using Shared.Abstractions;
using Integrations.SemanticKernel;
using System.Text.Json;

namespace Agents.FinancialCalculator;

public sealed record CalcRequest(decimal? principal, double? annualRatePct, int? termMonths, decimal? fees);
public sealed record CalcResult(decimal monthly, decimal totalInterest, decimal totalPayment);

public sealed class FinancialCalculatorAgent : IAgent
{
    public string Name => "FIN_CALC";
    private readonly ISkKernelFacade _kernel;

    public FinancialCalculatorAgent(ISkKernelFacade kernel) => _kernel = kernel;

    public async Task<AgentReply> HandleAsync(UserTurn turn, CancellationToken ct)
    {
        // Extract parameters from natural language
        var json = await _kernel.ExtractCalcParamsJsonAsync(turn.Text, ct);
        CalcRequest req;
        try { req = JsonSerializer.Deserialize<CalcRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new CalcRequest(null,null,null,null); }
        catch { req = new CalcRequest(null,null,null,null); }

        // Simple annuity formula (M = P * r / (1 - (1+r)^-n))
        var P = req.principal ?? 0m;
        var r = (decimal)((req.annualRatePct ?? 0.0) / 12.0 / 100.0);
        var n = Math.Max(1, req.termMonths ?? 1);

        decimal monthly;
        if (r == 0) monthly = P / n;
        else
        {
            var rf = (double)r;
            monthly = (decimal)((double)P * rf / (1 - Math.Pow(1 + rf, -n)));
        }

        var total = monthly * n + (req.fees ?? 0m);
        var interest = total - P;
        var result = new CalcResult(decimal.Round(monthly, 2), decimal.Round(interest, 2), decimal.Round(total, 2));

        var explanation = await _kernel.ExplainCalcAsync(result, ct);
        var text = explanation + "\n\n(JSON) " + JsonSerializer.Serialize(result);
        return new AgentReply(turn.ConversationId, Name, text);
    }
}
