\
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Project.Shared;

public class FaultMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _serviceName;

    public FaultMiddleware(RequestDelegate next, IConfiguration cfg)
    {
        _next = next;
        _serviceName = cfg["ServiceName"] ?? Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "unknown";
    }

    public async Task Invoke(HttpContext ctx)
    {
        var hdr = ctx.Request.Headers["X-Fault-Inject"].FirstOrDefault();
        var fault = FaultParser.ParseFromBase64Header(hdr);

        if (fault != null && FaultParser.MatchThisService(fault, _serviceName)
            && string.Equals(fault.AtCodeLayer, "controller", StringComparison.OrdinalIgnoreCase))
        {
            var prefix = FaultParser.ServicePrefix(_serviceName);
            switch (fault.Type)
            {
                case "delay":
                    if (fault.Params.DelayMs is int d) await Task.Delay(d, ctx.RequestAborted);
                    break;

                case "http_error":
                    {
                        var status = fault.Params.HttpStatus ?? 500;
                        var code = ErrorEnvelope.MapStatusToCode(status, prefix);
                        var msg = fault.Params.Message ?? DefaultMessage(status);
                        await ErrorEnvelope.WriteAsync(ctx, status, code, msg);
                        return;
                    }

                case "exception":
                    await ErrorEnvelope.WriteAsync(ctx, 500, $"{prefix}-SYS", fault.Params.Message ?? "ระบบขัดข้อง (simulated exception)");
                    return;

                case "business_error":
                    await ErrorEnvelope.WriteAsync(ctx, 400, $"{prefix}-VAL", fault.Params.Message ?? "รูปแบบข้อมูลไม่ถูกต้อง (simulated business error)");
                    return;

                case "cancel":
                    ctx.Abort();
                    return;

                case "timeout":
                    var t = fault.Params.TimeoutMs ?? 10000;
                    await Task.Delay(t, ctx.RequestAborted);
                    break;
            }
        }

        await _next(ctx);
    }

    private static string DefaultMessage(int status) => status switch
    {
        400 => "รูปแบบข้อมูลไม่ถูกต้อง",
        401 => "ไม่ได้รับอนุญาต",
        403 => "ถูกปฏิเสธการเข้าถึง",
        404 => "ไม่พบข้อมูล",
        409 => "เกิดข้อขัดแย้ง",
        422 => "ตรวจสอบข้อมูลไม่ผ่าน",
        429 => "ส่งคำขอถี่เกินไป",
        502 => "ช่องทางเชื่อมต่อขัดข้อง",
        503 => "บริการไม่พร้อมใช้งาน",
        _   => "ระบบขัดข้อง"
    };
}
