using LoanApp.MockApi.Models;

namespace LoanApp.MockApi.Services;

public class FaultRegistry
{
    private readonly object _lock = new();
    private FaultConfig _config = new() { Profiles = new() { ["default"] = new FaultProfile() } };
    private DateTime _scenarioStart = DateTime.UtcNow;

    public void Load(FaultConfig cfg)
    {
        lock (_lock)
        {
            _config = cfg;
            _scenarioStart = DateTime.UtcNow;
        }
    }

    public void Toggle(bool enabled)
    {
        lock (_lock) { _config.Enabled = enabled; }
    }

    public FaultConfig Snapshot()
    {
        lock (_lock) return _config;
    }

    public FaultProfile? Resolve(HttpContext ctx)
    {
        lock (_lock)
        {
            if (!_config.Enabled) return null;

            var method = ctx.Request.Method.ToUpperInvariant();
            var path = ctx.Request.Path.Value ?? "";

            if (ctx.Request.Headers.TryGetValue("X-Fault-Profile", out var hp))
            {
                if (_config.Profiles.TryGetValue(hp.ToString(), out var p)) return p;
            }

            var since = (int)(DateTime.UtcNow - _scenarioStart).TotalSeconds;
            var scheduled = _config.Schedule.FirstOrDefault(s => since >= s.FromSec && since < s.ToSec);
            if (scheduled is not null && _config.Profiles.TryGetValue(scheduled.Profile, out var ps)) return ps;

            var rule = _config.Routes.FirstOrDefault(r =>
                path.StartsWith(r.Path, StringComparison.OrdinalIgnoreCase) &&
                r.Methods.Any(m => m.Equals(method, StringComparison.OrdinalIgnoreCase)));

            if (rule is null) return _config.Profiles.TryGetValue("default", out var def) ? def : null;
            return _config.Profiles.TryGetValue(rule.Profile, out var prof) ? prof : null;
        }
    }
}