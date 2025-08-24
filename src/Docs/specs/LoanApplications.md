# Loan Applications API Spec

**OpenAPI 3.1.0 (Service: LoanApplicationsController)** — generated 2025-08-24

```yaml
openapi: 3.1.0
info: { title: Loan Applications Service, version: 1.0.0 }
servers: [ { url: http://localhost:5080/api/v1 } ]
paths:
  /loan-applications:
    post: { summary: Create application (Idempotent), responses: { '201': { description: Created }, '400': { description: Bad request }, '409': { description: Conflict } } }
    get: { summary: List applications, responses: { '200': { description: List } } }
  /loan-applications/{appId}:
    get: { summary: Get application, responses: { '200': { description: Application }, '404': { description: Not found } } }
    put: { summary: Update draft, responses: { '200': { description: Updated }, '404': { description: Not found } } }
  /loan-applications/{appId}/documents:
    post: { summary: Upload document, responses: { '200': { description: Uploaded } } }
    get: { summary: List documents, responses: { '200': { description: Docs } } }
  /loan-applications/{appId}/submit:
    post: { summary: Submit for underwriting, responses: { '202': { description: Under review }, '404': { description: Not found } } }
  /loan-applications/{appId}/status:
    get: { summary: Poll status, responses: { '200': { description: Status } } }
  /loan-applications/{appId}/accept-offer:
    post: { summary: Accept offer, responses: { '200': { description: Accepted } } }
  /loan-applications/{appId}/reject-offer:
    post: { summary: Reject offer, responses: { '200': { description: Rejected } } }
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
