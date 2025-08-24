using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1")]
public class ProfileController : ControllerBase
{
    [HttpGet("me")]
    public ActionResult<ProfileResponse> Me() => Ok(new ProfileResponse("u_001", "user@example.com", "+66123456789"));

    [HttpPut("me")]
    public IActionResult Update([FromBody] UpdateProfileRequest req) => Ok(new { updated = true });

    [HttpGet("me/banks")]
    public IActionResult Banks() => Ok(new[] { new { id = "bk1", bank = "KBank", account = "xxx-xxx-1234" } });

    [HttpPost("me/banks")]
    public IActionResult AddBank([FromBody] BankAccountRequest req) => Ok(new { id = "bk2", added = true });

    [HttpDelete("me/banks/{id}")]
    public IActionResult DelBank(string id) => Ok(new { id, removed = true });

    [HttpPut("me/security/pin")]
    public IActionResult ChangePin() => Ok(new { changed = true });

    [HttpPost("me/security/reset-password")]
    public IActionResult ResetPassword() => Ok(new { reset = "sent" });

    [HttpPut("me/preferences")]
    public IActionResult Prefs() => Ok(new { updated = true });

    [HttpGet("documents")]
    public IActionResult Documents() => Ok(new[] { new DocumentItem("d1", "receipt", "https://example.com/d1.pdf") });
}