public static class RegisterProcessEndpoints
{
    public static IEndpointRouteBuilder MapMemberProcessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/procapi")
                       .WithGroupName("Process");

        group.MapPost("/me", HandleRegisterProcess)
             .WithName("MemberProcess")
             .Produces<MemberInfoResponse>(StatusCodes.Status200OK)
             .Produces<ErrorModel>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> HandleRegisterProcess(
        MemberInfoRequest req,
        IHttpClientFactory http,
        HttpContext httpCtx)
    {
        using var c = await http.CreateClient("customer")
            .PostAsJsonAsync("/systemapi/customer", new { citizenId = req.CitizenId });
        if (!c.IsSuccessStatusCode)
        {
            await ErrorEnvelope.WriteAsync(ctx, 500, $"{prefix}-PROC", c?.Content?.ToString() ?? "ระบบขัดข้อง (customer exception)");
            return;
        }

        var member = await c.Content.ReadFromJsonAsync<MemberInfoResponse>();
        return Results.Ok(member);
    }
}

public record MemberInfoRequest(
    string CitizenId,
    string Username
);

/// <summary>
/// Minimal member/customer information
/// </summary>
public class MemberInfoResponse
{
    /// <summary>
    /// Unique user identifier (system generated)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Thai National ID (13 digits)
    /// </summary>
    public string CitizenId { get; set; } = string.Empty;

    /// <summary>
    /// First name (given name)
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name (family name)
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Primary mobile number (E.164 format)
    /// </summary>
    public string Mobile { get; set; } = string.Empty;

    /// <summary>
    /// Optional email address
    /// </summary>
    public string? Email { get; set; }
}