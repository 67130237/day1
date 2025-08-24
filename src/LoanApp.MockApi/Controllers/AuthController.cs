using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;
using LoanApp.MockApi.Services;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly InMemoryStore _store;
    public AuthController(InMemoryStore store) => _store = store;

    [HttpPost("register")]
    public ActionResult<TokenResponse> Register([FromBody] RegisterRequest req)
        => Ok(new TokenResponse(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N")));

    [HttpPost("login")]
    public ActionResult<TokenResponse> Login([FromBody] LoginRequest req)
        => Ok(new TokenResponse(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N")));

    [HttpPost("refresh")]
    public ActionResult<TokenResponse> Refresh()
        => Ok(new TokenResponse(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N")));

    [HttpPost("logout")]
    public IActionResult Logout() => Ok(new { ok = true });

    [HttpPost("otp/send")]
    public IActionResult SendOtp([FromBody] OtpSendRequest req) => Ok(new { sent = true, channel = req.Channel });

    [HttpPost("otp/verify")]
    public IActionResult VerifyOtp([FromBody] OtpVerifyRequest req) => Ok(new { verified = req.Code == "000000" ? false : true });

    [HttpPost("biometric/link")]
    public IActionResult LinkBiometric([FromBody] DeviceBindingRequest req) => Ok(new { linked = true, req.DeviceId });

    [HttpGet("devices")]
    public IActionResult Devices() => Ok(new[] { new { deviceId = "dev1", lastActive = DateTime.UtcNow } });

    [HttpDelete("devices/{deviceId}")]
    public IActionResult Revoke(string deviceId) => Ok(new { revoked = deviceId });
}