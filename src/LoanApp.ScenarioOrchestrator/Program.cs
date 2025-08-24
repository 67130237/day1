using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var config = Args.Parse(args);
var baseUrl = config.BaseUrl ?? "http://localhost:5080/api/v1";
var scenarioPath = config.ScenarioPath ?? Path.Combine(AppContext.BaseDirectory, "scenarios", "sample-scenario.yaml");

Directory.CreateDirectory(Path.GetDirectoryName(scenarioPath)!);

// Serilog setup (compact JSON)
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
    .MinimumLevel.Information()
    .CreateLogger();

Log.Information("Scenario Orchestrator starting... baseUrl={BaseUrl} scenario={ScenarioPath}", baseUrl, scenarioPath);

var scenario = ScenarioLoader.Load(scenarioPath);
var http = HttpClientFactory.CreateClient(baseUrl, config.TimeoutSeconds);

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

var runner = new Runner(http, scenario, Log.Logger);
await runner.RunAsync(cts.Token);

Log.Information("Scenario Orchestrator completed.");
return;

public sealed record Args(string? BaseUrl, string? ScenarioPath, int TimeoutSeconds)
{
    public static Args Parse(string[] args)
    {
        string? baseUrl = null, scenario = null;
        int timeout = 30;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--baseUrl": baseUrl = args[++i]; break;
                case "--scenario": scenario = args[++i]; break;
                case "--timeout": timeout = int.Parse(args[++i]); break;
            }
        }
        return new Args(baseUrl, scenario, timeout);
    }
}

public static class HttpClientFactory
{
    public static HttpClient CreateClient(string baseUrl, int timeoutSeconds)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
}

public sealed class Runner
{
    private readonly HttpClient _http;
    private readonly Scenario _scenario;
    private readonly ILogger _log;
    private static readonly JsonSerializerOptions json = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };

    public Runner(HttpClient http, Scenario scenario, ILogger log)
    {
        _http = http;
        _scenario = scenario;
        _log = log;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var tasks = new List<Task>();
        var totalUsers = _scenario.Users;
        var think = _scenario.ThinkTimeSeconds;
        var rnd = new Random();

        _log.Information("Run scenarioId={ScenarioId} users={Users} durationSec={Duration} rps={Rps}", _scenario.ScenarioId, totalUsers, _scenario.DurationSeconds, _scenario.Rps);

        using var sem = new SemaphoreSlim(_scenario.Rps); // throttle RPS

        for (int i = 0; i < totalUsers; i++)
        {
            var user = new SimUser($"u-{Guid.NewGuid():N}".Substring(0,8), rnd.Next(1, 9999).ToString("D4"));
            var feature = WeightedPick(_scenario.Features, rnd);
            tasks.Add(Task.Run(() => RunUserFlow(user, feature, sem, think, rnd, ct)));
            await Task.Delay(TimeSpan.FromMilliseconds(100), ct); // stagger users
        }

        // optional global duration
        var runTask = Task.WhenAll(tasks);
        if (_scenario.DurationSeconds > 0)
        {
            var delay = Task.Delay(TimeSpan.FromSeconds(_scenario.DurationSeconds), ct);
            var finished = await Task.WhenAny(runTask, delay);
            if (finished == delay)
            {
                _log.Information("Duration reached. Cancelling...");
                ct.ThrowIfCancellationRequested();
            }
        }
        else
        {
            await runTask;
        }
    }

    private async Task RunUserFlow(SimUser user, Feature feature, SemaphoreSlim sem, (int min,int max) think, Random rnd, CancellationToken ct)
    {
        var traceId = $"00-{Guid.NewGuid():N}-01";
        try
        {
            await sem.WaitAsync(ct);
            switch (feature.Name)
            {
                case "loan-apply":
                    await Flows.LoanApplyAsync(_http, user, _scenario.ScenarioId, traceId, _log, think, rnd, ct);
                    break;
                case "payment-due":
                    await Flows.PaymentDueAsync(_http, user, _scenario.ScenarioId, traceId, _log, think, rnd, ct);
                    break;
                case "browse-products":
                    await Flows.BrowseProductsAsync(_http, user, _scenario.ScenarioId, traceId, _log, think, rnd, ct);
                    break;
                default:
                    await Flows.BrowseProductsAsync(_http, user, _scenario.ScenarioId, traceId, _log, think, rnd, ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Flow failed feature={Feature} userId={UserId} traceId={TraceId}", feature.Name, user.UserId, traceId);
        }
        finally
        {
            sem.Release();
        }
    }

    private static Feature WeightedPick(List<Feature> features, Random rnd)
    {
        var sum = features.Sum(f => f.Weight);
        var roll = rnd.NextDouble() * sum;
        double cum = 0;
        foreach (var f in features)
        {
            cum += f.Weight;
            if (roll <= cum) return f;
        }
        return features[0];
    }
}

public static class Flows
{
    private static readonly JsonSerializerOptions json = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static async Task DelayThink((int min,int max) think, Random rnd, CancellationToken ct)
    {
        if (think.max <= 0) return;
        var ms = rnd.Next(think.min*1000, think.max*1000);
        await Task.Delay(ms, ct);
    }

    private static void Tag(HttpRequestMessage req, string scenarioId, string feature, string step, string traceId, SimUser user)
    {
        req.Headers.TryAddWithoutValidation("traceparent", traceId);
        req.Headers.TryAddWithoutValidation("X-Scenario-Id", scenarioId);
        req.Headers.TryAddWithoutValidation("X-User-Id", user.UserId);
        req.Headers.TryAddWithoutValidation("X-Session-Id", user.SessionId);
        req.Headers.TryAddWithoutValidation("X-Device-Id", $"dev-{user.SessionId}");
        req.Headers.TryAddWithoutValidation("X-Feature", feature);
        req.Headers.TryAddWithoutValidation("X-Flow-Step", step);
        // allow overriding fault at step level via scenario
        if (!string.IsNullOrEmpty(user.OverrideFaultProfile))
            req.Headers.TryAddWithoutValidation("X-Fault-Profile", user.OverrideFaultProfile);
    }

    private static async Task<HttpResponseMessage> SendAsync(HttpClient http, HttpRequestMessage req, ILogger log, string feature, string step, string traceId, SimUser user, CancellationToken ct)
    {
        var retry = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.RequestTimeout || r.StatusCode == (System.Net.HttpStatusCode)429)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(200), 3),
                (outcome, ts, attempt, _) =>
                    log.Warning("retry attempt={Attempt} feature={Feature} step={Step} status={Status} traceId={TraceId}",
                                attempt, feature, step, (outcome.Result?.StatusCode), traceId));

        return await retry.ExecuteAsync(ct => http.SendAsync(req, ct), ct);
    }

    public static async Task LoanApplyAsync(HttpClient http, SimUser user, string scenarioId, string traceId, ILogger log, (int min,int max) think, Random rnd, CancellationToken ct)
    {
        // 1. Browse products
        var req1 = new HttpRequestMessage(HttpMethod.Get, "loan-products");
        Tag(req1, scenarioId, "loan-apply", "products", traceId, user);
        await SendAsync(http, req1, log, "loan-apply", "products", traceId, user, ct);
        await DelayThink(think, rnd, ct);

        // 2. Calculator
        var req2 = new HttpRequestMessage(HttpMethod.Post, "loan-calculator");
        Tag(req2, scenarioId, "loan-apply", "calculator", traceId, user);
        req2.Content = new StringContent(JsonSerializer.Serialize(new { amount = rnd.Next(10000, 100000), tenorMonths = rnd.Next(6, 36), rateType = "effective" }), Encoding.UTF8, "application/json");
        await SendAsync(http, req2, log, "loan-apply", "calculator", traceId, user, ct);
        await DelayThink(think, rnd, ct);

        // 3. Create application (Idempotency-Key)
        var idem = Guid.NewGuid().ToString();
        var req3 = new HttpRequestMessage(HttpMethod.Post, "loan-applications");
        Tag(req3, scenarioId, "loan-apply", "create-application", traceId, user);
        req3.Headers.TryAddWithoutValidation("Idempotency-Key", idem);
        req3.Content = new StringContent(JsonSerializer.Serialize(new { productId = "pl-001", amount = rnd.Next(20000, 150000), tenorMonths = rnd.Next(6, 24), purpose = "general" }), Encoding.UTF8, "application/json");
        var resp3 = await SendAsync(http, req3, log, "loan-apply", "create-application", traceId, user, ct);
        var json3 = await resp3.Content.ReadAsStringAsync(ct);
        var appId = JsonDocument.Parse(json3).RootElement.GetProperty("applicationId").GetString() ?? "app_unk";
        await DelayThink(think, rnd, ct);

        // 4. Submit
        var req4 = new HttpRequestMessage(HttpMethod.Post, $"loan-applications/{appId}/submit");
        Tag(req4, scenarioId, "loan-apply", "submit", traceId, user);
        await SendAsync(http, req4, log, "loan-apply", "submit", traceId, user, ct);
        await DelayThink(think, rnd, ct);

        // 5. Poll status
        for (int i = 0; i < 3; i++)
        {
            var req5 = new HttpRequestMessage(HttpMethod.Get, $"loan-applications/{appId}/status");
            Tag(req5, scenarioId, "loan-apply", "status", traceId, user);
            await SendAsync(http, req5, log, "loan-apply", "status", traceId, user, ct);
            await Task.Delay(TimeSpan.FromMilliseconds(rnd.Next(300,900)), ct);
        }

        // 6. Accept offer (optimistically)
        var req6 = new HttpRequestMessage(HttpMethod.Post, $"loan-applications/{appId}/accept-offer");
        Tag(req6, scenarioId, "loan-apply", "accept-offer", traceId, user);
        await SendAsync(http, req6, log, "loan-apply", "accept-offer", traceId, user, ct);
    }

    public static async Task PaymentDueAsync(HttpClient http, SimUser user, string scenarioId, string traceId, ILogger log, (int min,int max) think, Random rnd, CancellationToken ct)
    {
        var loanId = "ln_1001"; // mock known id in API
        // 1. Next due
        var r1 = new HttpRequestMessage(HttpMethod.Get, $"billing/{loanId}/next-due");
        Tag(r1, scenarioId, "payment-due", "next-due", traceId, user);
        await SendAsync(http, r1, log, "payment-due", "next-due", traceId, user, ct);
        await DelayThink(think, rnd, ct);

        // 2. Create payment intent
        var r2 = new HttpRequestMessage(HttpMethod.Post, "payments/intent");
        Tag(r2, scenarioId, "payment-due", "intent", traceId, user);
        r2.Headers.TryAddWithoutValidation("Idempotency-Key", Guid.NewGuid().ToString());
        r2.Content = new StringContent(JsonSerializer.Serialize(new { loanId = loanId, method = "QR_PROMPTPAY" }), Encoding.UTF8, "application/json");
        var resp2 = await SendAsync(http, r2, log, "payment-due", "intent", traceId, user, ct);
        var body2 = await resp2.Content.ReadAsStringAsync(ct);
        string paymentId = "pay_demo";
        try { paymentId = JsonDocument.Parse(body2).RootElement.GetProperty("paymentId").GetString() ?? paymentId; } catch {}

        await DelayThink(think, rnd, ct);

        // 3. Poll payment status
        for (int i = 0; i < 3; i++)
        {
            var r3 = new HttpRequestMessage(HttpMethod.Get, $"payments/{paymentId}");
            Tag(r3, scenarioId, "payment-due", "status", traceId, user);
            await SendAsync(http, r3, log, "payment-due", "status", traceId, user, ct);
            await Task.Delay(TimeSpan.FromMilliseconds(rnd.Next(300,900)), ct);
        }

        // 4. Receipt
        var r4 = new HttpRequestMessage(HttpMethod.Get, $"payments/{paymentId}/receipt");
        Tag(r4, scenarioId, "payment-due", "receipt", traceId, user);
        await SendAsync(http, r4, log, "payment-due", "receipt", traceId, user, ct);
    }

    public static async Task BrowseProductsAsync(HttpClient http, SimUser user, string scenarioId, string traceId, ILogger log, (int min,int max) think, Random rnd, CancellationToken ct)
    {
        var r1 = new HttpRequestMessage(HttpMethod.Get, "loan-products");
        Tag(r1, scenarioId, "browse-products", "list", traceId, user);
        await SendAsync(http, r1, log, "browse-products", "list", traceId, user, ct);
        await DelayThink(think, rnd, ct);

        var r2 = new HttpRequestMessage(HttpMethod.Post, "loan-calculator");
        Tag(r2, scenarioId, "browse-products", "calculator", traceId, user);
        r2.Content = new StringContent(JsonSerializer.Serialize(new { amount = rnd.Next(5000, 50000), tenorMonths = rnd.Next(6, 24), rateType = "flat" }), Encoding.UTF8, "application/json");
        await SendAsync(http, r2, log, "browse-products", "calculator", traceId, user, ct);
    }
}

public sealed record SimUser(string UserId, string SessionId)
{
    public string? OverrideFaultProfile { get; init; }
}

public static class ScenarioLoader
{
    public static Scenario Load(string path)
    {
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, DefaultYaml);
        }
        var yaml = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var s = deserializer.Deserialize<Scenario>(yaml);
        return s;
    }

    public static string DefaultYaml => """
scenarioId: payday-peak-01
users: 50
rps: 10
durationSeconds: 0   # 0 = wait all users finish, >0 = stop after seconds
thinkTimeSeconds: { min: 1, max: 3 }
features:
  - name: loan-apply
    weight: 0.3
  - name: payment-due
    weight: 0.5
  - name: browse-products
    weight: 0.2
""";
}

public sealed class Scenario
{
    public string ScenarioId { get; set; } = "demo";
    public int Users { get; set; } = 10;
    public int Rps { get; set; } = 5;
    public int DurationSeconds { get; set; } = 0;
    public (int min, int max) ThinkTimeSeconds { get; set; } = (1,3);
    public List<Feature> Features { get; set; } = new();
}

public sealed class Feature
{
    public string Name { get; set; } = "browse-products";
    public double Weight { get; set; } = 1.0;
}