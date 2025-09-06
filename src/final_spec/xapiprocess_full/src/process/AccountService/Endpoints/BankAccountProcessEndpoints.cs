public static class BankAccountProcessEndpoints
{
    public static IEndpointRouteBuilder MapBankAccountProcessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/procapi")
                       .WithGroupName("Process");

        group.MapGet("/accounts", HandleProcess)
             .WithName("BankAccountProcess")
             .Produces<BankAccountsResponse>(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IResult> HandleProcess(
        GetBankAccountsRequest req,
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
            .PostAsJsonAsync("/systemapi/accounts", new { citizenId = req.CitizenId });
        if (!o.IsSuccessStatusCode)
        {
            await ErrorEnvelope.WriteAsync(ctx, 500, $"{prefix}-PROC", o?.Content?.ToString() ?? "ระบบขัดข้อง (account exception)");
            return;
        }
        
        var bankAccounts = await s.Content.ReadFromJsonAsync<List<BankAccount>();
        return Results.Ok(new BankAccountsResponse {
            Accounts = bankAccounts
        });
    }
}

public record GetBankAccountsRequest(
    string CitizenId
);


public class BankAccountsResponse
{
    public List<BankAccount> Accounts { get; set; } 
}

public class BankAccount
{
    /// <summary>
    /// Unique identifier for the account (UUID or internal ID).
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Account number (masked or full depending on policy).
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the account (e.g., "Savings Account").
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Type of account (e.g., saving, current, loan).
    /// </summary>
    public string AccountType { get; set; } = string.Empty;

    /// <summary>
    /// Current balance of the account.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Currency code (ISO 4217).
    /// </summary>
    public string Currency { get; set; } = "THB";

    /// <summary>
    /// Status of the account (e.g., active, frozen, closed).
    /// </summary>
    public string Status { get; set; } = "active";

    /// <summary>
    /// Date when the account was opened.
    /// </summary>
    public DateTime OpenedDate { get; set; }

    /// <summary>
    /// Optional last transaction date (nullable).
    /// </summary>
    public DateTime? LastTransactionDate { get; set; }
}