using Microsoft.AspNetCore.Mvc;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    [HttpGet("applications")]
    public IActionResult Review([FromQuery] string status = "pending")
        => Ok(new { items = new[]{ new { applicationId="app_aaaaaa", status } } });

    [HttpPost("applications/{appId}/approve")]
    public IActionResult Approve(string appId) => Ok(new { appId, decision = "approved" });

    [HttpPost("applications/{appId}/reject")]
    public IActionResult Reject(string appId) => Ok(new { appId, decision = "rejected" });

    [HttpPost("applications/{appId}/offer")]
    public IActionResult Offer(string appId, [FromBody] object body) => Ok(new { appId, offered = true });

    [HttpPost("loans/{loanId}/disburse")]
    public IActionResult Disburse(string loanId) => Ok(new { loanId, status = "queued" });

    [HttpPost("loans/{loanId}/remind")]
    public IActionResult Remind(string loanId) => Ok(new { loanId, sent = true });

    [HttpPost("loans/{loanId}/writeoff")]
    public IActionResult Writeoff(string loanId) => Ok(new { loanId, status = "written_off" });
}