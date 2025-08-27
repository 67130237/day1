\
namespace Project.Shared;

public record FaultParams(int? DelayMs = null, int? HttpStatus = null, string? Message = null, int? TimeoutMs = null, string? SqlState = null);
public record FaultInject(string Type, string AtService, string AtCodeLayer, FaultParams Params);

public static class FaultParser
{
    public static FaultInject? ParseFromBase64Header(string? b64)
    {
        if (string.IsNullOrWhiteSpace(b64)) return null;
        try
        {
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64));
            return System.Text.Json.JsonSerializer.Deserialize<FaultInject>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }

    public static bool MatchThisService(FaultInject f, string serviceName)
        => string.Equals(f.AtService, serviceName, StringComparison.OrdinalIgnoreCase);

    public static string ServicePrefix(string serviceName) => serviceName.ToLowerInvariant() switch
    {
        "dopa" => "DOPA",
        "otp" => "OTP",
        "notification" => "NOTI",
        "appsettings" => "APPS",
        "cms" => "CMS",
        _ => serviceName.ToUpperInvariant()
    };
}
