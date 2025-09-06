using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Net.Http;

namespace Shared.Infrastructure;

public static class HttpResilience
{
    public static AsyncRetryPolicy<HttpResponseMessage> GetDefaultPolicy(ILogger logger) =>
        Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
              .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(2),
                  (result, ts, retry, ctx) =>
                  {
                      logger.LogWarning("Retry {Retry} after {Delay}s due to {StatusCode}", retry, ts.TotalSeconds, result.Result?.StatusCode);
                  });
}
