using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class KycController : ControllerBase
{
    private static string _status = "unverified";

    [HttpGet("profile")]
    public ActionResult<KycProfileResponse> Profile() => Ok(new KycProfileResponse(_status));

    [HttpPost("submit")]
    public IActionResult Submit([FromBody] KycSubmitRequest req)
    {
        _status = "pending";
        return Accepted(new { status = _status });
    }

    [HttpPost("documents")]
    public IActionResult UploadDocs() => Ok(new { uploaded = true });

    [HttpPost("face/verify")]
    public IActionResult FaceVerify() { _status = "verified"; return Ok(new { status = _status }); }

    [HttpGet("check")]
    public IActionResult Check() => Ok(new { status = _status });
}

[ApiController]
[Route("api/v1/consent")]
public class ConsentController : ControllerBase
{
    [HttpPost("credit-check")]
    public IActionResult CreditConsent([FromBody] CreditConsentRequest req) => Ok(new { accepted = req.Accepted, at = DateTime.UtcNow });

    [HttpGet("credit-check/status")]
    public IActionResult CreditStatus() => Ok(new { score = 720, bureauRef = "BR-" + Guid.NewGuid().ToString("N")[..8] });
}