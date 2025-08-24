using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Controllers;

[ApiController]
[Route("api/v1/loan-applications")]
public class LoanApplicationsController : ControllerBase
{
    private static readonly Dictionary<string, string> _apps = new();

    [HttpPost]
    public ActionResult<LoanApplicationResponse> Create([FromBody] LoanApplicationCreateRequest req, [FromHeader(Name="Idempotency-Key")] string? idemKey)
    {
        var id = "app_" + Guid.NewGuid().ToString("N")[..6];
        _apps[id] = "pending";
        return Created($"/api/v1/loan-applications/{id}", new LoanApplicationResponse(id, "pending"));
    }

    [HttpGet]
    public IActionResult List([FromQuery] string? status = null)
    {
        var data = _apps.Select(kv => new { applicationId = kv.Key, status = kv.Value });
        if (!string.IsNullOrEmpty(status)) data = data.Where(x => x.status == status);
        return Ok(new { items = data, meta = new { total = _apps.Count } });
    }

    [HttpGet("{appId}")]
    public IActionResult Detail(string appId)
        => _apps.ContainsKey(appId) ? Ok(new { applicationId = appId, status = _apps[appId], timeline = new[] { "created", "under_review" } }) : NotFound();

    [HttpPut("{appId}")]
    public IActionResult Update(string appId, [FromBody] UpdateApplicationRequest req)
    {
        if (!_apps.ContainsKey(appId)) return NotFound();
        return Ok(new { applicationId = appId, updated = true });
    }

    [HttpPost("{appId}/documents")]
    public IActionResult UploadDoc(string appId) => Ok(new { applicationId = appId, uploaded = true });

    [HttpGet("{appId}/documents")]
    public IActionResult ListDocs(string appId) => Ok(new[] { new { id = "doc1", type = "payslip" } });

    [HttpPost("{appId}/submit")]
    public IActionResult Submit(string appId)
    {
        if (!_apps.ContainsKey(appId)) return NotFound();
        _apps[appId] = "under_review";
        return Accepted(new { applicationId = appId, status = "under_review" });
    }

    [HttpGet("{appId}/status")]
    public IActionResult Status(string appId)
    {
        if (!_apps.ContainsKey(appId)) return NotFound();
        // promote to approved mock
        if (_apps[appId] == "under_review") _apps[appId] = "approved";
        return Ok(new { applicationId = appId, status = _apps[appId] });
    }

    [HttpPost("{appId}/accept-offer")]
    public IActionResult AcceptOffer(string appId)
    {
        if (!_apps.ContainsKey(appId)) return NotFound();
        return Ok(new { applicationId = appId, accepted = true, next = new { contractId = "ct_" + appId[4..] } });
    }

    [HttpPost("{appId}/reject-offer")]
    public IActionResult RejectOffer(string appId) => Ok(new { applicationId = appId, accepted = false });
}