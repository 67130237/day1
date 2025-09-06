using System.Reflection;

namespace Project.Shared;

public static class SystemServiceEndpoints
{
    public static string ServiceNameURL { get; private set; } =
        Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "unhknown";
}
