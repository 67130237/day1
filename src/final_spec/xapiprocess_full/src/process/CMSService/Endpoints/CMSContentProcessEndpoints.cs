public static class RegisterProcessEndpoints
{
    public static IEndpointRouteBuilder MapRegisterProcessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/process")
                       .WithGroupName("Process");

        group.MapPost("/register", HandleRegisterProcess)
             .WithName("RegisterProcess")
             .Produces<RegisterAggregate>(StatusCodes.Status200OK)
             .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> HandleRegisterProcess(
        RegisterRequest req,
        IHttpClientFactory http,
        HttpContext httpCtx)
    {
        // validate
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.Mobile)) errors.Add("mobile is required.");
        if (string.IsNullOrWhiteSpace(req.NationalId)) errors.Add("nationalId is required.");
        if (string.IsNullOrWhiteSpace(req.FirstName)) errors.Add("firstName is required.");
        if (string.IsNullOrWhiteSpace(req.LastName)) errors.Add("lastName is required.");
        if (errors.Count > 0) return Results.BadRequest(new ErrorResponse("INVALID_REQUEST", string.Join(" ", errors)));

        var traceId = httpCtx.Request.Headers["xapp-trace-id"].ToString();

        // 1) settings (optional)
        SettingsResponse? settingsResp = null;
        try
        {
            using var s = await http.CreateClient("settings").GetAsync("/api/settings/app");
            if (s.IsSuccessStatusCode)
                settingsResp = await s.Content.ReadFromJsonAsync<SettingsResponse>();
        }
        catch { /* ไม่ critical */ }

        // 2) otp/send (ถ้าขอให้ส่ง)
        OtpSendResponse? otpSend = null;
        if (req.SendOtp == true)
        {
            using var o = await http.CreateClient("otp")
                .PostAsJsonAsync("/api/otp/send", new { mobile = req.Mobile });
            if (!o.IsSuccessStatusCode)
                return Results.BadRequest(await ToError(o, "OTP_SEND_FAILED"));

            otpSend = await o.Content.ReadFromJsonAsync<OtpSendResponse>();
            if (req.OnlySendOtp == true)
            {
                return Results.Ok(new RegisterAggregate
                {
                    TraceId = traceId,
                    Step = "otp_sent",
                    Settings = settingsResp,
                    Otp = new() { Sent = true, Token = otpSend?.Token }
                });
            }
        }

        if (string.IsNullOrWhiteSpace(req.OtpCode))
            return Results.BadRequest(new ErrorResponse("OTP_REQUIRED", "otpCode is required to continue registration."));

        // 3) otp/verify
        {
            using var v = await http.CreateClient("otp")
                .PostAsJsonAsync("/api/otp/verify", new { mobile = req.Mobile, code = req.OtpCode, token = otpSend?.Token ?? req.OtpToken });
            if (!v.IsSuccessStatusCode)
                return Results.BadRequest(await ToError(v, "OTP_VERIFY_FAILED"));
        }

        // 4) dopa/verify-identity
        {
            using var d = await http.CreateClient("dopa")
                .PostAsJsonAsync("/api/dopa/verify-identity", new
                {
                    nationalId = req.NationalId,
                    firstName = req.FirstName,
                    lastName = req.LastName,
                    birthDate = req.BirthDate
                });
            if (!d.IsSuccessStatusCode)
                return Results.BadRequest(await ToError(d, "IDENTITY_VERIFY_FAILED"));
        }

        // 5) register/create-account
        RegisterCreateResponse? created;
        {
            using var r = await http.CreateClient("register")
                .PostAsJsonAsync("/api/register/create-account", new
                {
                    mobile = req.Mobile,
                    nationalId = req.NationalId,
                    firstName = req.FirstName,
                    lastName = req.LastName,
                    pin = req.Pin
                });
            if (!r.IsSuccessStatusCode)
                return Results.BadRequest(await ToError(r, "REGISTER_CREATE_FAILED"));

            created = await r.Content.ReadFromJsonAsync<RegisterCreateResponse>();
        }

        return Results.Ok(new RegisterAggregate
        {
            TraceId = traceId,
            Step = "completed",
            Settings = settingsResp,
            Otp = new() { Sent = otpSend is not null, Token = otpSend?.Token, Verified = true },
            Register = created
        });
    }

    private static async Task<ErrorResponse> ToError(HttpResponseMessage resp, string fallbackCode)
    {
        try
        {
            var body = await resp.Content.ReadAsStringAsync();
            var parsed = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(body, new()
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
            if (parsed is not null && !string.IsNullOrWhiteSpace(parsed.Code))
                return parsed with { HttpStatus = (int)resp.StatusCode };

            return new ErrorResponse(fallbackCode, $"downstream returned {(int)resp.StatusCode}. body={Truncate(body, 500)}", (int)resp.StatusCode);
        }
        catch
        {
            return new ErrorResponse(fallbackCode, $"downstream returned {(int)resp.StatusCode}", (int)resp.StatusCode);
        }
    }

    private static string Truncate(string s, int n)
        => string.IsNullOrEmpty(s) ? s : (s.Length <= n ? s : s.Substring(0, n) + "...");
}

public sealed class Downstreams
{
    public string Otp { get; set; } = "http://localhost:7001";
    public string Dopa { get; set; } = "http://localhost:7002";
    public string Register { get; set; } = "http://localhost:7003";
    public string Settings { get; set; } = "http://localhost:7004";
}

public record RegisterRequest(
    string Mobile,
    string NationalId,
    string FirstName,
    string LastName,
    string? BirthDate,
    string? Pin,
    bool? SendOtp,
    bool? OnlySendOtp,
    string? OtpCode,
    string? OtpToken
);

public record ErrorResponse(string Code, string Message, int? HttpStatus = null);

public record SettingsResponse(string? AppVersion, bool? EnableRegistration, Dictionary<string, string>? Flags);

public record OtpSendResponse(string? Token, int? ExpireSeconds);

public record RegisterCreateResponse(string CustomerId, string? WelcomeStatus);



public record RegisterAggregate
{
    public string TraceId { get; set; } = default!;
    public string Step { get; set; } = default!;
    public SettingsResponse? Settings { get; set; }
    public OtpAggregate Otp { get; set; } = new();
    public RegisterCreateResponse? Register { get; set; }
}

public record OtpAggregate
{
    public bool Sent { get; set; }
    public string? Token { get; set; }
    public bool Verified { get; set; }
}
