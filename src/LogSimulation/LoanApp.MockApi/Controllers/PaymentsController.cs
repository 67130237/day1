using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;
using LoanApp.MockApi.Services;
using System.Text.Json;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IdempotencyCache _idem;
    private readonly EventQueues _queues;
    private readonly ILogger<PaymentsController> _log;

    public PaymentsController(IdempotencyCache idem, EventQueues queues, ILogger<PaymentsController> log)
    {
        _idem = idem; _queues = queues; _log = log;
    }

    [HttpPost("intent")]
    public ActionResult<PaymentIntentResponse> CreateIntent([FromBody] PaymentIntentRequest req, [FromHeader(Name="Idempotency-Key")] string? idemKey)
    {
        if (!string.IsNullOrWhiteSpace(idemKey) && _idem.TryGet(idemKey!, out var cached))
            return Content(cached!, "application/json");

        var pid = "pay_" + Guid.NewGuid().ToString("N")[..6];
        var resp = new PaymentIntentResponse(pid, "pending", "bankapp://qr/123", DateTime.UtcNow.AddMinutes(10));

        // enqueue webhook in 5-15s
        var fireAt = DateTime.UtcNow.AddSeconds(new Random().Next(5, 15));
        _queues.PaymentIntents.Enqueue(new PaymentIntentEvent(pid, fireAt));

        var json = JsonSerializer.Serialize(resp);
        if (!string.IsNullOrWhiteSpace(idemKey)) _idem.Set(idemKey!, json);

        _log.LogInformation("payment intent created {PaymentId} fireAt={FireAt}", pid, fireAt);
        return Ok(resp);
    }

    [HttpPost("charge")]
    public IActionResult Charge([FromBody] PaymentChargeRequest req, [FromHeader(Name="Idempotency-Key")] string? idemKey)
    {
        if (!string.IsNullOrWhiteSpace(idemKey) && _idem.TryGet(idemKey!, out var cached))
            return Content(cached!, "application/json");

        var pid = "pay_" + Guid.NewGuid().ToString("N")[..6];
        var json = JsonSerializer.Serialize(new { paymentId = pid, status = "pending" });
        if (!string.IsNullOrWhiteSpace(idemKey)) _idem.Set(idemKey!, json);

        var fireAt = DateTime.UtcNow.AddSeconds(new Random().Next(5, 15));
        _queues.PaymentIntents.Enqueue(new PaymentIntentEvent(pid, fireAt));
        _log.LogInformation("payment charge enqueued {PaymentId} fireAt={FireAt}", pid, fireAt);

        return Content(json, "application/json");
    }

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
    private readonly ILogger<PaymentWebhooksController> _log;
    public PaymentWebhooksController(ILogger<PaymentWebhooksController> log) { _log = log; }

    [HttpPost("provider")]
    public IActionResult Provider([FromBody] object body)
    {
        _log.LogInformation("PSP webhook: {Body}", System.Text.Json.JsonSerializer.Serialize(body));
        return Ok(new { ok = true });
    }
}