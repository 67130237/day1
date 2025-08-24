using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1/support")]
public class SupportController : ControllerBase
{
    [HttpGet("faq")]
    public ActionResult<IEnumerable<object>> Faq() => Ok(new[] { new { q = "ชำระเงินอย่างไร?", a = "สแกน QR หรือบัตรเดบิต" } });

    [HttpPost("tickets")]
    public ActionResult<TicketItem> Create([FromBody] TicketCreateRequest req) => Ok(new TicketItem("t1", req.Subject, "open"));

    [HttpGet("tickets")]
    public ActionResult<IEnumerable<TicketItem>> List() => Ok(new[] { new TicketItem("t1", "ปัญหาการชำระเงิน", "open") });

    [HttpGet("tickets/{id}")]
    public ActionResult<TicketItem> Detail(string id) => Ok(new TicketItem(id, "ปัญหาการชำระเงิน", "open"));

    [HttpPost("tickets/{id}/messages")]
    public IActionResult Reply(string id) => Ok(new { id, sent = true });
}