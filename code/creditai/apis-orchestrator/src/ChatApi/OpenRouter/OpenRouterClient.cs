//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using ChatApi.Shared.Infrastructure;

//namespace Integrations.OpenRouter;

//public sealed class OpenRouterClient
//{
//    private readonly IHttpClientFactory _factory;
//    private readonly IConfiguration _cfg;
//    private readonly string _model;

//    public static void Add(IServiceCollection services, IConfiguration cfg, string? name = null)
//    {
//        // Register resilient client
//        services.AddResilientHttpClient(name ?? "openrouter");
//        services.AddSingleton<OpenRouterClient>();
//    }

//    public OpenRouterClient(IHttpClientFactory factory, IConfiguration cfg)
//    {
//        _factory = factory;
//        _cfg = cfg;
//        _model = cfg["OpenRouter:Model"] ?? "openai/gpt-4o-mini"; // placeholder; user can change
//    }

//    public async Task<string> ChatAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
//    {
//        var apiKey = _cfg["OpenRouter:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
//        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("OpenRouter API key is missing.");

//        var http = _factory.CreateClient("openrouter");
//        http.BaseAddress = new Uri(_cfg["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/");
//        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
//        http.DefaultRequestHeaders.Add("HTTP-Referer", _cfg["OpenRouter:Referer"] ?? "https://example.com");
//        http.DefaultRequestHeaders.Add("X-Title", _cfg["OpenRouter:Title"] ?? "CreditAI");

//        var payload = new
//        {
//            model = _model,
//            messages = new object[]
//            {
//                new { role = "system", content = systemPrompt },
//                new { role = "user", content = userPrompt }
//            },
//            temperature = 0.2
//        };

//        var json = JsonSerializer.Serialize(payload);
//        var req = new StringContent(json, Encoding.UTF8, "application/json");
//        var resp = await http.PostAsync("chat/completions", req, ct);
//        resp.EnsureSuccessStatusCode();
//        var body = await resp.Content.ReadAsStringAsync(ct);

//        using var doc = JsonDocument.Parse(body);
//        var text = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
//        return text ?? string.Empty;
//    }

//    public async Task<float[]> EmbedAsync(string input, CancellationToken ct = default)
//    {
//        var apiKey = _cfg["OpenRouter:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
//        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("OpenRouter API key is missing.");

//        var http = _factory.CreateClient("openrouter");
//        http.BaseAddress = new Uri(_cfg["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/");
//        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

//        var model = _cfg["OpenRouter:EmbeddingModel"] ?? "text-embedding-3-small"; // placeholder
//        var payload = new { model, input };
//        var json = JsonSerializer.Serialize(payload);
//        var req = new StringContent(json, Encoding.UTF8, "application/json");
//        var resp = await http.PostAsync("embeddings", req, ct);
//        resp.EnsureSuccessStatusCode();
//        var body = await resp.Content.ReadAsStringAsync(ct);

//        using var doc = JsonDocument.Parse(body);
//        var arr = doc.RootElement.GetProperty("data")[0].GetProperty("embedding").EnumerateArray();
//        return arr.Select(x => x.GetSingle()).ToArray();
//    }
//}
