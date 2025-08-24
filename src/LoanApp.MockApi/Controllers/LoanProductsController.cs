using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1")]
public class LoanProductsController : ControllerBase
{
    private static readonly List<LoanProduct> Products = new()
    {
        new("pl-001", "Personal Loan Standard", "personal", 5000, 500000, 15.99),
        new("pl-002", "Motor Title Loan", "title", 10000, 300000, 18.50),
    };

    [HttpGet("loan-products")]
    public ActionResult<IEnumerable<LoanProduct>> GetAll() => Ok(Products);

    [HttpGet("loan-products/{id}")]
    public ActionResult<LoanProduct> GetOne(string id)
        => Products.FirstOrDefault(p => p.Id == id) is { } p ? Ok(p) : NotFound();

    [HttpPost("loan-calculator")]
    public ActionResult<LoanCalculatorResponse> Calc([FromBody] LoanCalculatorRequest req)
    {
        var rate = 0.015m; // mock 1.5% monthly
        var monthly = decimal.Round((req.Amount * rate) + (req.Amount / req.TenorMonths), 2);
        var totalInt = monthly * req.TenorMonths - req.Amount;
        return Ok(new LoanCalculatorResponse(monthly, totalInt));
    }
}