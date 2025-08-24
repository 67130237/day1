# Loan Products API Spec

**OpenAPI 3.1.0 (Service: LoanProductsController)** — generated 2025-08-24

```yaml
openapi: 3.1.0
info: { title: Loan Products Service, version: 1.0.0 }
servers: [ { url: http://localhost:5080/api/v1 } ]
paths:
  /loan-products: { get: { summary: List loan products, responses: { '200': { description: Product list } } } }
  /loan-products/{id}: { get: { summary: Get product, responses: { '200': { description: Product }, '404': { description: Not found } } } }
  /loan-calculator: { post: { summary: Calculate monthly payment, responses: { '200': { description: Calculation } } } }
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
