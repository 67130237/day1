using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ChatApi.Shared.Infrastructure;

namespace ChatApi.Mcp;

public abstract class McpClientBase
{
    protected const string DefaultClientName = "mcp";
    protected readonly IHttpClientFactory Factory;
    protected readonly IConfiguration Cfg;
    protected readonly string ServiceKey;

    public static void Add(IServiceCollection services, string clientName = DefaultClientName)
    {
        services.AddResilientHttpClient(clientName);
    }

    protected McpClientBase(IHttpClientFactory factory, IConfiguration cfg, string serviceKey)
    {
        Factory = factory;
        Cfg = cfg;
        ServiceKey = serviceKey;
    }

    protected HttpClient CreateClient(string? clientName = null)
    {
        var http = Factory.CreateClient(clientName ?? DefaultClientName);
        var baseUrl = Cfg[$"{ServiceKey}:BaseUrl"] ?? throw new InvalidOperationException($"{ServiceKey}:BaseUrl is missing");
        http.BaseAddress = new Uri(baseUrl);
        var apiKey = Cfg[$"{ServiceKey}:ApiKey"] ?? Environment.GetEnvironmentVariable($"{ServiceKey.ToUpperInvariant().Replace(':','_')}_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }
        return http;
    }

    protected static StringContent Json(object payload) =>
        new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    protected static async Task<T> ReadJsonAsync<T>(HttpResponseMessage resp, CancellationToken ct)
    {
        resp.EnsureSuccessStatusCode();
        var str = await resp.Content.ReadAsStringAsync(ct);
        return System.Text.Json.JsonSerializer.Deserialize<T>(str, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
