# auth-service @ 1.0.0

## Overview

บริการยืนยันตัวตน: ลงทะเบียน, เข้าสู่ระบบ (PIN/Biometric), refresh/revoke token, ข้อมูลผู้ใช้ปัจจุบัน

## Headers
 

- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


## Endpoints


### POST /xapi/v1/auth/register

สร้างบัญชีผู้ใช้ใหม่ พร้อม policy: ต้องผ่าน OTP + DOPA (ผ่าน orchestrator) ก่อน activate

**Security:** ไม่ต้องใช้ JWT  
**Idempotency:** รองรับ (ใช้ business key: citizenId+mobile)

**Request Schema**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| mobile | string | yes | E.164 | เบอร์มือถือผู้ใช้ |
| email | string | no | RFC 5322 | อีเมล (ถ้ามี) |
| citizenId | string | yes | 13 digits | เลขบัตรประชาชน |
| firstName | string | yes | - | ชื่อจริง |
| lastName | string | yes | - | นามสกุล |
| pin | string | yes | 6 digits | PIN เริ่มต้น (จะต้อง enrol ใหม่หลังยืนยัน) |
| acceptTerms | boolean | yes | - | ยอมรับข้อตกลงการใช้บริการ |
| channel | string | yes | mobile|web | ช่องทางที่ลงทะเบียน |
| metadata | object | no | - | ข้อมูลอุปกรณ์ (deviceId, os, appVersion) |



**Response 201**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| userId | string | yes | - | รหัสผู้ใช้ |
| status | string | yes | pending|active | สถานะหลังสมัคร |
| nextStep | string | yes | - | เช่น 'otp', 'dopa-verify' |
| traceid | string | yes | GUID | trace id |



**Business Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| AUTH-VAL-001 | 400 | ข้อมูลลงทะเบียนไม่ครบ/ไม่ถูกต้อง | ฟิลด์หาย, รูปแบบผิด | No |
| AUTH-VAL-002 | 409 | ผู้ใช้นี้มีอยู่แล้ว | ซ้ำ citizenId/mobile/email | No |
| AUTH-EXT-001 | 502 | บริการ DOPA/OTP ไม่พร้อมใช้งาน | external timeout/down | Yes |
| AUTH-SYS-001 | 500 | ระบบขัดข้อง | ข้อผิดพลาดภายใน | Yes |



```bash
curl -X POST "https://api.example.com/xapi/v1/auth/register" \
  -H "Content-Type: application/json" \
  -H "xapp-trace-id: 63d8a49b-1c7e-4f0e-96c0-83d9f7d8ae01" \
  -d '{
  "mobile": "+66812345678",
  "email": "user@example.com",
  "citizenId": "1234567890123",
  "firstName": "Somchai",
  "lastName": "Dee",
  "pin": "123456",
  "acceptTerms": true,
  "channel": "mobile",
  "metadata": {
    "deviceId": "dev-001",
    "os": "Android",
    "appVersion": "1.0.0"
  }
}'
```


### POST /xapi/v1/auth/login/pin

เข้าสู่ระบบด้วย PIN

**Security:** ไม่ต้องใช้ JWT  
**Idempotency:** ไม่จำเป็น

**Request Schema**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| mobile | string | yes | E.164 | เบอร์ที่ผูกบัญชี |
| pin | string | yes | 6 digits | รหัส PIN |
| deviceId | string | yes | - | อุปกรณ์ที่ผูก (device binding) |
| biometricCapable | boolean | no | - | อุปกรณ์รองรับ biometric หรือไม่ |



**Response 200**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| accessToken | string | yes | JWT | โทเคนสำหรับเรียก API |
| refreshToken | string | yes | opaque | โทเคนต่ออายุ session |
| expiresIn | number | yes | seconds | อายุ access token |
| mfaRequired | boolean | yes | - | ต้อง OTP เพิ่มหรือไม่ |
| traceid | string | yes | GUID | trace |



**Business Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| AUTH-AUTH-001 | 401 | PIN ไม่ถูกต้อง | pin mismatch | No |
| AUTH-AUTH-002 | 403 | บัญชีถูกล็อคชั่วคราว | ผิดหลายครั้งเกินกำหนด | Yes |
| AUTH-VAL-003 | 400 | อุปกรณ์ยังไม่ผูกกับบัญชี | device binding not found | No |
| AUTH-EXT-002 | 502 | บริการ OTP ไม่พร้อมใช้งาน | ต้อง OTP แต่ช่องทางล่ม | Yes |
| AUTH-SYS-002 | 500 | ระบบขัดข้อง | internal error | Yes |



### POST /xapi/v1/auth/login/biometric

เข้าสู่ระบบด้วย biometric assertion (หลังจาก enrol แล้ว)

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| deviceId | string | yes | - | อุปกรณ์ |
| assertion | string | yes | base64 | signed assertion จาก device |
| nonce | string | yes | - | ป้องกัน replay |



**Response/Errors** เช่นเดียวกับ PIN แต่แทนที่ด้วย biometric เฉพาะกรณี:
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| AUTH-AUTH-003 | 401 | ยังไม่ได้ลงทะเบียน Biometric | not enrolled | No |
| AUTH-AUTH-004 | 401 | Assertion ไม่ถูกต้อง | signature verify fail | No |



### POST /xapi/v1/auth/token/refresh

ต่ออายุ access token จาก refresh token

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| refreshToken | string | yes | opaque | โทเคนต่ออายุ |



**Response**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| accessToken | string | yes | JWT | ใหม่ |
| expiresIn | number | yes | seconds | อายุใหม่ |
| traceid | string | yes | GUID | trace |



**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| AUTH-AUTH-005 | 401 | Refresh token หมดอายุ | expired | No |
| AUTH-AUTH-006 | 401 | Refresh token ถูกเพิกถอน | revoked | No |



### POST /xapi/v1/auth/token/revoke

เพิกถอน refresh token ทั้งหมดของอุปกรณ์/ผู้ใช้

**Security:** ต้องใช้ JWT

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| deviceId | string | no | - | ถ้าไม่ระบุ = ทุกอุปกรณ์ |



**Response 200**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| revoked | boolean | yes | - | ผลการเพิกถอน |
| traceid | string | yes | GUID | trace |



## Non-Functional Requirements

- **Latency**: p95 ≤ 200–400ms ตามประเภท (read/write)
- **Scalability**: รองรับ horizontal scaling, stateless, connection pooling
- **Observability**: Elastic APM, log โครงสร้าง, trace-span ผูกกับ `xapp-trace-id`, metrics ต่อ endpoint
- **Security**: TLS 1.2+, JWT + scopes, input validation, output encoding, PII masking ใน logs
- **Idempotency**: รองรับสำหรับคำสั่งที่สำคัญ (โอน/ถอน/แจ้งเตือน/ปล่อยเงิน)
- **Resilience**: circuit breaker, retry with backoff, DLQ สำหรับงาน async, graceful degradation
- **Caching**: ETag/Cache-Control, response caching (ที่เหมาะสม)
- **Compliance**: audit trail, data retention policy, consent/opt-in (สำหรับ marketing)

