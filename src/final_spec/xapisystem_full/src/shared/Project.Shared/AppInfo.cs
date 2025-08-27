\
using System.Reflection;

namespace Project.Shared;

public static class AppInfo
{
    public static string ServiceName { get; private set; } =
        Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "unknown";

    public static string Version { get; } =
        Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "1.0.0";
}
