namespace LoanApp.MockApi.Models;

public record FaultProfile
{
    public int LatencyMs { get; init; } = 0;
    public double ErrorRate { get; init; } = 0.0; // 0..1
    public int AbortHttpStatus { get; init; } = 0; // 0=off
}

public record RouteFaultRule
{
    public string Path { get; init; } = "/";
    public string[] Methods { get; init; } = new[] { "GET" };
    public string Profile { get; init; } = "default";
}

public record FaultScheduleItem
{
    public int FromSec { get; init; }
    public int ToSec { get; init; }
    public string Profile { get; init; } = "default";
}

public record FaultConfig
{
    public Dictionary<string, FaultProfile> Profiles { get; init; } = new();
    public List<RouteFaultRule> Routes { get; init; } = new();
    public List<FaultScheduleItem> Schedule { get; init; } = new();
    public bool Enabled { get; set; } = true;
}

public record ScenarioConfig
{
    public string ScenarioId { get; init; } = "default";
    public int Users { get; init; } = 10;
    public int Rps { get; init; } = 5;
    public int DurationSec { get; init; } = 60;
}