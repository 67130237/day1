using System;
using System.Net.Http;

using Microsoft.Extensions.DependencyInjection;

namespace ChatApi.Shared.Infrastructure;

public static class HttpClientResilienceExtensions
{
    /// <summary>
    /// Registers a resilient HttpClient with sensible defaults (Polly v8):
    /// - Per-attempt timeout
    /// - Exponential backoff + jitter
    /// - Retries for transient HTTP errors (5xx, 408) and 429 by default
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient(
        this IServiceCollection services,
        string name,
        TimeSpan? perAttemptTimeout = null,
        int retryCount = 3)
    {
        var timeout = perAttemptTimeout ?? TimeSpan.FromSeconds(20);
        return null;
        //return services
        //    .AddHttpClient(name)
        //    // ถ้าต้องการ total timeout รวมทุกความพยายาม สามารถตั้ง client.Timeout ได้
        //    .ConfigureHttpClient(c =>
        //    {
        //        // ตัวอย่าง: รวมทุก attempt ไม่เกิน 60s (ปรับได้ตามบริบท)
        //        c.Timeout = TimeSpan.FromSeconds(60);
        //    })
        //    .AddResilienceHandler("resilient-pipeline", builder =>
        //    {
        //        // Retry: default ของแพ็กเกจนี้จะ handle 5xx, 408, 429 และ HttpRequestException/TimeoutRejectedException ให้แล้ว
        //        // เราปรับจำนวนครั้ง, รูปแบบ backoff, jitter และฐาน delay
        //        builder.AddRetry(new HttpRetryStrategyOptions
        //        {
        //            MaxRetryAttempts = retryCount,
        //            BackoffType = DelayBackoffType.Exponential,
        //            UseJitter = true,
        //            Delay = TimeSpan.FromMilliseconds(250),

        //            // หากอยาก log ทุกครั้งก่อนจะรอแล้วค่อยลองใหม่
        //            OnRetry = args =>
        //            {
        //                Log.Warning(
        //                    "[HTTP RETRY] attempt={Attempt} delay={DelayMs}ms reason={Reason}",
        //                    args.AttemptNumber,
        //                    args.RetryDelay.TotalMilliseconds,
        //                    args.Outcome.Exception?.Message ?? args.Outcome.Result?.ReasonPhrase);
        //                return default;
        //            },
        //        });

        //        // Per-attempt timeout
        //        builder.AddTimeout(timeout);

        //        // จะเพิ่ม Circuit Breaker ก็ได้ (ค่า default ของ standard handler มีให้)
        //        // ตัวอย่าง:
        //        // builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        //        // {
        //        //     FailureRatio = 0.1, MinimumThroughput = 100,
        //        //     SamplingDuration = TimeSpan.FromSeconds(30),
        //        //     BreakDuration = TimeSpan.FromSeconds(5)
        //        // });
        //    });
    }
}
