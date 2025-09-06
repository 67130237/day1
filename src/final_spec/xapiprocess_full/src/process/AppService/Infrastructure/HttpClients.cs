using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

public static class HttpClients
{
    public static IServiceCollection AddDownstreamHttpClients(this IServiceCollection services, Downstreams ds)
    {
        services.AddHttpClient("settings", c =>
        {
            c.BaseAddress = new Uri(ds.Settings);
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<TraceIdForwardingHandler>();

        services.AddHttpClient("otp", c =>
        {
            c.BaseAddress = new Uri(ds.Otp);
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<TraceIdForwardingHandler>();

        services.AddHttpClient("dopa", c =>
        {
            c.BaseAddress = new Uri(ds.Dopa);
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<TraceIdForwardingHandler>();

        services.AddHttpClient("register", c =>
        {
            c.BaseAddress = new Uri(ds.Register);
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddHttpMessageHandler<TraceIdForwardingHandler>();

        return services;
    }
}