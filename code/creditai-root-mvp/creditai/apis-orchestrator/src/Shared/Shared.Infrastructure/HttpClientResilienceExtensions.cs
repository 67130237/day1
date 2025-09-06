using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Polly.Extensions.Http;
using Serilog;

namespace Shared.Infrastructure;

public static class HttpClientResilienceExtensions
{
    /// <summary>
    /// Registers a resilient HttpClient with sensible defaults:
    /// - Timeout per attempt
    /// - Retry on transient errors (5xx, 408, DNS)
    /// - Jittered backoff
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient(
        this IServiceCollection services,
        string name,
        TimeSpan? perAttemptTimeout = null,
        int retryCount = 3)
    {
        var timeout = perAttemptTimeout ?? TimeSpan.FromSeconds(20);

        // Retry policy with exponential backoff + jitter
        var retry: IAsyncPolicy<HttpResponseMessage> = HttpPolicyExtensions
            .HandleTransientHttpError() // 5xx + 408 + network failures
            .OrResult(msg => (int)msg.StatusCode == 429) // Too Many Requests
            .WaitAndRetryAsync(
                retryCount,
                attempt => TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                (outcome, ts, attempt, ctx) =>
                {
                    Log.Warning("[HTTP RETRY] attempt={Attempt} delay={DelayMs}ms reason={Reason}",
                        attempt, ts.TotalMilliseconds, outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase);
                });

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);

        return services.AddHttpClient(name)
            .AddPolicyHandler(retry)
            .AddPolicyHandler(timeoutPolicy);
    }
}
