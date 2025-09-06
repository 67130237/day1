using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Project.Shared;
namespace RegisterProcessApi.Models;

public class ErrorModel
{
    /// <summary>
    /// Business or system error code (e.g., PREG-VAL-001).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human readable error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Trace identifier (matches xapp-trace-id header).
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details (can be object or empty).
    /// </summary>
    public object Details { get; set; } = new { };
}
