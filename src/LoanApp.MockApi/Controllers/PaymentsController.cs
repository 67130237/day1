using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1/payments")]
public class PaymentsController : ControllerBase
{
    [HttpPost("intent")]
    public ActionResult<PaymentIntentResponse> CreateIntent([FromBody] PaymentIntentRequest req)
        => Ok(new PaymentIntentResponse("pay_" + Guid.NewGuid().ToString("N")[..6], "pending", "bankapp://qr/123", DateTime.UtcNow.AddMinutes(10)));

    [HttpPost("charge")]
    public IActionResult Charge([FromBody] PaymentChargeRequest req, [FromHeader(Name="Idempotency-Key")] string? idemKey)
        => Ok(new { paymentId = "pay_" + Guid.NewGuid().ToString("N")[..6], status = "pending" });

    [HttpGet("{paymentId}")]
    public ActionResult<PaymentStatusResponse> Status(string paymentId) => Ok(new PaymentStatusResponse(paymentId, "success"));

    [HttpGet("{paymentId}/receipt")]
    public ActionResult<ReceiptResponse> Receipt(string paymentId) => Ok(new ReceiptResponse(paymentId, $"https://example.com/receipt/{paymentId}.pdf"));

    [HttpPost("autodebit/setup")]
    public IActionResult SetupAuto() => Ok(new { active = true });

    [HttpGet("autodebit/status")]
    public IActionResult AutoStatus() => Ok(new { status = "active" });

    [HttpDelete("autodebit")]
    public IActionResult AutoCancel() => Ok(new { status = "inactive" });
}

[ApiController]
[Route("api/v1/webhooks/payments")]
public class PaymentWebhooksController : ControllerBase
{
    [HttpPost("provider")]
    public IActionResult Provider() => Ok(new { ok = true });
}