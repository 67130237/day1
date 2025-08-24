using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1")]
public class BillingController : ControllerBase
{
    [HttpGet("billing/{loanId}/next-due")]
    public ActionResult<InvoiceResponse> NextDue(string loanId) => Ok(new InvoiceResponse("inv_2001", "due", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), 10500m));

    [HttpGet("billing/{loanId}/invoices")]
    public IActionResult Invoices(string loanId) => Ok(new[] { new InvoiceResponse("inv_2001", "due", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), 10500m) });

    [HttpGet("billing/invoices/{invoiceId}")]
    public IActionResult Invoice(string invoiceId) => Ok(new InvoiceResponse(invoiceId, "due", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), 10500m));
}