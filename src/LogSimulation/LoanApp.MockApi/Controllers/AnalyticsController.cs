using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1")]
public class AnalyticsController : ControllerBase
{
    [HttpGet("analytics/repayment")]
    public ActionResult<RepaymentAnalytics> Repayment()
        => Ok(new RepaymentAnalytics(35000m, 120000m));

    [HttpGet("credit/health")]
    public ActionResult<CreditHealth> CreditHealth()
        => Ok(new CreditHealth(720, "A"));
}