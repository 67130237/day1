using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Shared;
public class TraceIdForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TraceIdForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.Request.Headers.TryGetValue("xapp-trace-id", out var traceId))
        {
            // ใส่ trace id ไปยัง request ต่อไป
            if (!request.Headers.Contains("xapp-trace-id"))
            {
                request.Headers.Add("xapp-trace-id", traceId.ToString());
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddTraceIdForwarding(this IHttpClientBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        return builder.AddHttpMessageHandler<TraceIdForwardingHandler>();
    }
}


//var builder = WebApplication.CreateBuilder(args);

//// Register Handler
//builder.Services.AddTransient<TraceIdForwardingHandler>();

//// ใช้กับ HttpClientFactory
//builder.Services.AddHttpClient("DownstreamService", c =>
//{
//    c.BaseAddress = new Uri("http://localhost:5001"); // service ต่อไป
//})
//.AddTraceIdForwarding();

//var app = builder.Build();

//app.MapGet("/call-next", async (IHttpClientFactory factory) =>
//{
//    var client = factory.CreateClient("DownstreamService");
//    var res = await client.GetAsync("/api/test");
//    return Results.Content(await res.Content.ReadAsStringAsync());
//});

//app.Run();