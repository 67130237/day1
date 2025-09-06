using Polly;
using Polly.Extensions.Http;
namespace CreditAI.Shared.Infrastructure;

public static class HttpClientResilienceExtensions
{
    public static IHttpClientBuilder AddResilientHttpClient(
        this IServiceCollection services,
        string name,
        TimeSpan? perAttemptTimeout = null,
        int retryCount = 3)
    {
        var timeout = perAttemptTimeout ?? TimeSpan.FromSeconds(20);

        IAsyncPolicy<HttpResponseMessage> retry = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => (int)msg.StatusCode == 429)
            .WaitAndRetryAsync(
                retryCount,
                attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)) +
                           TimeSpan.FromMilliseconds(Random.Shared.Next(0, 250)));

        var builder = services.AddHttpClient(name, c =>
        {
            c.Timeout = timeout;
        });
        builder.AddPolicyHandler(retry);
        return builder;
    }
}
