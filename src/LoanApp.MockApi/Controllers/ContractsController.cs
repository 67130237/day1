using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1")]
public class ContractsController : ControllerBase
{
    [HttpGet("contracts/{contractId}")]
    public ActionResult<ContractResponse> Get(string contractId)
        => Ok(new ContractResponse(contractId, "pending_sign", $"https://example.com/contract/{contractId}.pdf"));

    [HttpGet("contracts/{contractId}/pdf")]
    public ActionResult GetPdf(string contractId)
        => Ok(new { url = $"https://example.com/contract/{contractId}.pdf" });

    [HttpPost("contracts/{contractId}/esign")]
    public IActionResult Esign(string contractId) => Ok(new { contractId, status = "signed" });

    [HttpGet("disbursements/{contractId}")]
    public IActionResult DisburseStatus(string contractId) => Ok(new { contractId, status = "sent" });
}