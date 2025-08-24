# Auth API Spec

**OpenAPI 3.1.0 (Service: AuthController)** — generated 2025-08-24

```yaml
openapi: 3.1.0
info:
  title: Auth Service
  version: 1.0.0
  description: Authentication & Onboarding endpoints
servers:
  - url: http://localhost:5080/api/v1
tags:
  - name: auth
paths:
  /auth/register:
    post:
      tags: [auth]
      summary: Register user
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/RegisterRequest'
      responses:
        '200':
          description: Registered and auto-logged-in
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/TokenResponse'
        '400': { $ref: '#/components/responses/BadRequest' }
  /auth/login:
    post:
      tags: [auth]
      summary: Login
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/LoginRequest'
      responses:
        '200':
          description: Tokens
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/TokenResponse'
        '401': { $ref: '#/components/responses/Unauthorized' }
  /auth/refresh:
    post:
      tags: [auth]
      summary: Refresh token
      responses:
        '200':
          description: New tokens
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/TokenResponse'
  /auth/logout:
    post:
      tags: [auth]
      summary: Logout (revoke refresh)
      responses:
        '200': { description: Ok }
  /auth/otp/send:
    post:
      tags: [auth]
      summary: Send OTP
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/OtpSendRequest'
      responses:
        '200': { description: OTP sent }
  /auth/otp/verify:
    post:
      tags: [auth]
      summary: Verify OTP
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/OtpVerifyRequest'
      responses:
        '200': { description: Verified }
        '400': { $ref: '#/components/responses/BadRequest' }
  /auth/biometric/link:
    post:
      tags: [auth]
      summary: Link biometric with device
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/DeviceBindingRequest'
      responses:
        '200': { description: Linked }
  /auth/devices:
    get:
      tags: [auth]
      summary: List logged-in devices
      responses:
        '200': { description: Device list }
  /auth/devices/{deviceId}:
    delete:
      tags: [auth]
      summary: Revoke a device
      responses:
        '200': { description: Revoked }
components:
  responses:
    BadRequest:
      description: Bad request
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/Error'
    Unauthorized:
      description: Unauthorized
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/Error'
  schemas:
    RegisterRequest:
      type: object
      required: [email, phone, password]
      properties:
        email: { type: string, format: email }
        phone: { type: string }
        password: { type: string, minLength: 6 }
    LoginRequest:
      type: object
      required: [username, password]
      properties:
        username: { type: string }
        password: { type: string }
    OtpSendRequest:
      type: object
      required: [channel, destination]
      properties:
        channel: { type: string, enum: [SMS, Email] }
        destination: { type: string }
    OtpVerifyRequest:
      type: object
      required: [code]
      properties:
        code: { type: string }
    DeviceBindingRequest:
      type: object
      required: [deviceId]
      properties:
        deviceId: { type: string }
    TokenResponse:
      type: object
      properties:
        accessToken: { type: string }
        refreshToken: { type: string }
        tokenType: { type: string, default: Bearer }
    Error:
      type: object
      properties:
        error: { type: string }
        message: { type: string }
        traceId: { type: string }
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
