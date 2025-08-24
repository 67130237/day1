using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1/loans")]
public class LoansController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => Ok(new { items = new[]{ new { loanId="ln_1001", productId="pl-001" } }, meta = new { total = 1 } });

    [HttpGet("{loanId}")]
    public ActionResult<LoanResponse> Get(string loanId) => Ok(new LoanResponse(loanId, 120000m, 1500m));

    [HttpGet("{loanId}/schedule")]
    public IActionResult Schedule(string loanId)
    {
        var list = Enumerable.Range(1, 12).Select(i =>
            new ScheduleItem(i, DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(i)), 9000m, 1500m, 120000m - i*9000m));
        return Ok(list);
    }

    [HttpGet("{loanId}/transactions")]
    public IActionResult Tx(string loanId)
        => Ok(new[] { new { id = "tx1", type = "payment", amount = 10500m, at = DateTime.UtcNow } });

    [HttpPost("{loanId}/early-quote")]
    public ActionResult<QuoteResponse> EarlyQuote(string loanId) => Ok(new QuoteResponse(118000m, DateTime.UtcNow.AddHours(2)));

    [HttpPost("{loanId}/partial-quote")]
    public ActionResult<QuoteResponse> PartialQuote(string loanId, [FromBody] QuoteRequest req) => Ok(new QuoteResponse(req.Amount ?? 10000m, DateTime.UtcNow.AddHours(1)));

    [HttpPost("{loanId}/restructure/request")]
    public IActionResult Restructure(string loanId) => Accepted(new { loanId, status = "under_review" });
}