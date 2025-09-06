public static class BankAccountProcessEndpoints
{
    public static IEndpointRouteBuilder MapTransferHistoryProcessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/procapi")
                       .WithGroupName("Process");

        group.MapGet("/transfer-history", HandleProcess)
             .WithName("TransferHistoryProcess")
             .Produces<TransferHistoriesResponse>(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IResult> HandleProcess(
        GetTransferHistoryRequest req,
        IHttpClientFactory http,
        HttpContext httpCtx)
    {
        using var c = await http.CreateClient("customer")
            .PostAsJsonAsync("/systemapi/customer", new { citizenId = req.CitizenId });
        if (!c.IsSuccessStatusCode)
        {
            await ErrorEnvelope.WriteAsync(ctx, 500, $"{prefix}-PROC", c?.Content?.ToString() ?? "ระบบขัดข้อง (customer exception)");
            return;
        }

        using var o = await http.CreateClient("account")
            .PostAsJsonAsync("/systemapi/transfer-history", new { accountId = req.AccountId });
        if (!o.IsSuccessStatusCode)
        {
            await ErrorEnvelope.WriteAsync(ctx, 500, $"{prefix}-PROC", o?.Content?.ToString() ?? "ระบบขัดข้อง (account exception)");
            return;
        }
        
        var histories = await o.Content.ReadFromJsonAsync<List<TransferHistory>();
        return Results.Ok(new TransferHistoriesResponse {
            Histories = histories
        });
    }
}

public record GetTransferHistoryRequest(
    string AccountId,
    string CitizenId
);

public class TransferHistoriesResponse
{
    public List<TransferHistory> Histories { get; set; } 
}


public class TransferHistory
{
    /// <summary>
    /// Unique identifier for the transfer transaction.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the transfer was made.
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Amount of money transferred.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (ISO 4217), e.g., THB.
    /// </summary>
    public string Currency { get; set; } = "THB";

    /// <summary>
    /// Source account number (masked or full depending on policy).
    /// </summary>
    public string FromAccount { get; set; } = string.Empty;

    /// <summary>
    /// Destination account number (masked or full).
    /// </summary>
    public string ToAccount { get; set; } = string.Empty;

    /// <summary>
    /// Name of the beneficiary (destination account owner).
    /// </summary>
    public string BeneficiaryName { get; set; } = string.Empty;

    /// <summary>
    /// Description or transfer note.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Transaction status (e.g., success, pending, failed).
    /// </summary>
    public string Status { get; set; } = "success";

    /// <summary>
    /// Bank code or identifier of the destination bank (for interbank transfer).
    /// </summary>
    public string DestinationBankCode { get; set; } = string.Empty;

    /// <summary>
    /// Fee charged for this transaction (if any).
    /// </summary>
    public decimal Fee { get; set; }
}