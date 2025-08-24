using System.Net.Http.Json;

namespace LoanApp.MockApi.Services;

public class WebhookEmulator : BackgroundService
{
    private readonly ILogger<WebhookEmulator> _log;
    private readonly EventQueues _queues;
    private readonly IHttpClientFactory _http;

    public WebhookEmulator(ILogger<WebhookEmulator> log, EventQueues queues, IHttpClientFactory http)
    { _log = log; _queues = queues; _http = http; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        var client = _http.CreateClient("self");
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            while (_queues.PaymentIntents.TryPeek(out var ev))
            {
                if (DateTime.UtcNow >= ev.FireAt && _queues.PaymentIntents.TryDequeue(out var evt))
                {
                    try
                    {
                        var payload = new { paymentId = evt.PaymentId, status = "success", provider = "mockpsp" };
                        await client.PostAsJsonAsync("/api/v1/webhooks/payments/provider", payload, stoppingToken);
                        _log.LogInformation("Webhook fired for {PaymentId}", evt.PaymentId);
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning(ex, "Webhook fire failed");
                    }
                }
                else break;
            }
        }
    }
}