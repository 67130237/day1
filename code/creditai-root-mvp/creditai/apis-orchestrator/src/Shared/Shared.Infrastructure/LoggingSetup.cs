using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Shared.Infrastructure;

public static class LoggingSetup
{
    /// <summary>
    /// Configure Serilog for console logging with reasonable defaults.
    /// </summary>
    public static void AddSerilogMinimal(this IServiceCollection services, IConfiguration cfg)
    {
        var level = cfg["Logging:Serilog:MinimumLevel"] ?? "Information";
        var parsed = Enum.TryParse<LogEventLevel>(level, true, out var lvl) ? lvl : LogEventLevel.Information;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(parsed)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        services.AddLogging(lb =>
        {
            lb.ClearProviders();
            lb.AddSerilog(dispose: true);
        });
    }
}
