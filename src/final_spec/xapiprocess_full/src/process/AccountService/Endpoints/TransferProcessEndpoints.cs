using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

public static class TransferProcessEndpoints
{
    public static IEndpointRouteBuilder MapTransferProcessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/procapi")
                       .WithGroupName("Process");

        // ใช้ POST สำหรับ process ที่มี body
        group.MapPost("/transfer", HandleProcess)
             .WithName("TransferProcess")
             .Produces<TransferResponse>(StatusCodes.Status200OK)
             .Produces<ErrorModel>(StatusCodes.Status400BadRequest)
             .Produces<ErrorModel>(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> HandleProcess(
        TransferRequest req,
        IHttpClientFactory http,
        HttpContext httpCtx)
    {
        const string prefix = "XFER"; // ใช้ prefix สำหรับ code ภายใน

        // 1) customer: ตรวจสอบลูกค้า/บัญชีผู้ใช้
        using var custResp = await http.CreateClient("customer")
            .PostAsJsonAsync("/systemapi/customer", new { citizenId = req.CitizenId });

        if (!custResp.IsSuccessStatusCode)
        {
            var body = await SafeReadAsync(custResp);
            return ErrorEnvelope.ToResult(httpCtx, StatusCodes.Status500InternalServerError,
                $"{prefix}-CUSTOMER", $"ระบบขัดข้อง (customer). {body}");
        }

        // 2) disbursement (หรือ payment gateway): ยืนยันคำสั่งโอน
        using var disbResp = await http.CreateClient("disbursement")
            .PostAsJsonAsync("/systemapi/transfer-confirm", new
            {
                accountId = req.AccountId,
                toAccount = req.ToAccount,
                toBankCode = req.ToBankCode,
                amount = req.Amount,
                currency = req.Currency ?? "THB",
                note = req.Note
            });

        if (!disbResp.IsSuccessStatusCode)
        {
            var body = await SafeReadAsync(disbResp);
            return ErrorEnvelope.ToResult(httpCtx, StatusCodes.Status500InternalServerError,
                $"{prefix}-CONFIRM", $"ระบบขัดข้อง (disbursement). {body}");
        }

        // 3) account: สร้างประวัติการโอน (บันทึกทรานแซคชัน)
        using var histCreateResp = await http.CreateClient("account")
            .PostAsJsonAsync("/systemapi/create-transfer-history", new
            {
                accountId = req.AccountId,
                toAccount = req.ToAccount,
                toBankCode = req.ToBankCode,
                amount = req.Amount,
                currency = req.Currency ?? "THB",
                note = req.Note
            });

        if (!histCreateResp.IsSuccessStatusCode)
        {
            var body = await SafeReadAsync(histCreateResp);
            return ErrorEnvelope.ToResult(httpCtx, StatusCodes.Status500InternalServerError,
                $"{prefix}-HISTORY", $"ระบบขัดข้อง (account history). {body}");
        }

        // 4) notification: แจ้งเตือนสำเร็จ
        using var notiResp = await http.CreateClient("notification")
            .PostAsJsonAsync("/systemapi/pushnotification", new
            {
                accountId = req.AccountId,
                title = "Transfer completed",
                message = $"โอนเงินสำเร็จ {req.Amount:0.00} {(req.Currency ?? "THB")} ไปยัง {req.ToAccount}"
            });

        if (!notiResp.IsSuccessStatusCode)
        {
            var body = await SafeReadAsync(notiResp);
            return ErrorEnvelope.ToResult(httpCtx, StatusCodes.Status500InternalServerError,
                $"{prefix}-NOTIFY", $"ระบบขัดข้อง (notification). {body}");
        }

        return Results.Ok(new TransferResponse
        {
            Histories = histories
        });
    }

    private static async Task<string> SafeReadAsync(HttpResponseMessage resp)
    {
        try { return await resp.Content.ReadAsStringAsync(); }
        catch { return $"HTTP {(int)resp.StatusCode}"; }
    }
}

// ========================= Models =========================

public class TransferRequest
{
    public string CitizenId { get; set; } = string.Empty;  // สำหรับตรวจลูกค้า
    public string AccountId { get; set; } = string.Empty;  // บัญชีต้นทาง
    public string ToAccount { get; set; } = string.Empty;  // บัญชีปลายทาง
    public string ToBankCode { get; set; } = string.Empty; // ธนาคารปลายทาง
    public decimal Amount { get; set; }
    public string? Currency { get; set; } = "THB";
    public string? Note { get; set; }
}

public class TransferResponse
{
    public string TransactionId { get; set; } = string.Empty;
    public DateTimeOffset TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "THB";
    public string FromAccount { get; set; } = string.Empty;
    public string ToAccount { get; set; } = string.Empty;
    public string ToBankCode { get; set; } = string.Empty;
    public string Status { get; set; } = "success";
    public decimal Fee { get; set; }
    public string? Note { get; set; }
}
