using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Shared;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ElasticsearchClient _elastic;

    public RequestLoggingMiddleware(RequestDelegate next, ElasticsearchClient elastic)
    {
        _next = next;
        _elastic = elastic;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        // ✅ เก็บ Request Body
        string requestBody = "";
        if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
        {
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true
            );
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        // ✅ เก็บ Response Body
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // อ่าน Response Body
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        // ✅ สร้าง Log Object
        var requestLog = new RequestLog
        {
            TraceId = context.TraceIdentifier,
            Method = context.Request.Method,
            Path = context.Request.Path + context.Request.QueryString,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers["User-Agent"].ToString(),
            RequestHeaders = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            RequestBody = requestBody,
            ResponseHeaders = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            ResponseBody = responseBodyText,
            Timestamp = DateTime.UtcNow
        };

        // ✅ ส่งเข้า Elasticsearch
        try
        {
            await _elastic.IndexAsync(requestLog, i => i.Index("request-logs"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ElasticError] {ex.Message}");
        }

        // ✅ คืนค่า Response กลับไปให้ client
        await responseBody.CopyToAsync(originalBodyStream);
    }
}

public record RequestLog
{
    public string TraceId { get; set; }
    public string Method { get; set; }
    public string Path { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public Dictionary<string, string> RequestHeaders { get; set; }
    public string RequestBody { get; set; }
    public Dictionary<string, string> ResponseHeaders { get; set; }
    public string ResponseBody { get; set; }
    public DateTime Timestamp { get; set; }
}


public static class ElasticsearchLoggingExtensions
{
    public static IServiceCollection AddElasticsearchLogging(this IServiceCollection services, string defaultIndex = "request-logs")
    {
        string uri = "http://localhost:9200";
        var settings = new ElasticsearchClientSettings(new Uri(uri))
            .DefaultIndex(defaultIndex);

        var client = new ElasticsearchClient(settings);

        services.AddSingleton(client);
        return services;
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}