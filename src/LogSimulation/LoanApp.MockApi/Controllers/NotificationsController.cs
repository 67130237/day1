using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1")]
public class NotificationsController : ControllerBase
{
    [HttpGet("notifications")]
    public ActionResult<IEnumerable<NotificationItem>> List()
        => Ok(new[] { new NotificationItem("n1", "due", "ถึงกำหนดชำระ", "ยอดงวด 10,500 บาท", false) });

    [HttpPost("notifications/{id}/read")]
    public IActionResult Read(string id) => Ok(new { id, read = true });

    [HttpPost("push/register")]
    public IActionResult RegisterPush() => Ok(new { registered = true });
}