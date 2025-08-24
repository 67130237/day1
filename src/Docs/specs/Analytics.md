# Analytics & Credit Health API Spec

**OpenAPI 3.1.0 (Service: AnalyticsController)** — generated 2025-08-24

```yaml
openapi: 3.1.0
info: { title: Analytics & Credit Health Service, version: 1.0.0 }
servers: [ { url: http://localhost:5080/api/v1 } ]
paths:
  /analytics/repayment: { get: { summary: Repayment analytics, responses: { '200': { description: Analytics } } } }
  /credit/health: { get: { summary: Credit health (non-bureau), responses: { '200': { description: Health } } } }
components: { schemas: { Error: { type: object, properties: { error: {type: string}, message: {type: string}, traceId: {type: string} } } } }
```

### Common Error Model

```yaml
components:
  schemas:
    Error:
      type: object
      required: [error, message]
      properties:
        error:
          type: string
          description: Machine-readable error code (e.g., validation_error, not_found, unauthorized, injected_fault)
        message:
          type: string
          description: Human-readable description
        traceId:
          type: string
          description: Distributed trace id (W3C traceparent correlation)
```

**HTTP Status Codes (ทั่วไป)**
- `200 OK` – สำเร็จ
- `201 Created` – สร้างรายการใหม่สำเร็จ (คืน Location header)
- `202 Accepted` – รับคำขอและกำลังประมวลผลแบบ async
- `400 Bad Request` – ค่าที่ส่งไม่ถูกต้อง (คืน `Error`)
- `401 Unauthorized` – ไม่ได้ยืนยันตัวตน / token หมดอายุ
- `403 Forbidden` – ไม่มีสิทธิ์
- `404 Not Found` – ไม่พบรายการ
- `409 Conflict` – ขัดแย้ง (เช่น Idempotency ซ้ำ)
- `422 Unprocessable Entity` – ตรวจสอบแล้วไม่ผ่าน
- `429 Too Many Requests` – ถูกจำกัดอัตรา
- `500 Internal Server Error` – ความผิดพลาดภายในระบบ
- `503 Service Unavailable` – ผู้ให้บริการภายนอก/ระบบย่อยล่ม (เช่น PSP/DB)
