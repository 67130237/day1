# customer-service @ 1.0.0

## Overview

ข้อมูลลูกค้า, โปรไฟล์, user settings (ที่ไม่ใช่ notification preference)

## Headers


- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


## Endpoints


### GET /xapi/v1/customers/me

ดึงข้อมูลโปรไฟล์ของผู้ใช้ปัจจุบัน

**Security:** JWT (scope: `profile:read`)

**Response 200**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| userId | string | yes | - | รหัสผู้ใช้ |
| fullName | string | yes | - | ชื่อ-สกุล |
| mobile | string | yes | E.164 | เบอร์ |
| email | string | no | RFC | อีเมล |
| locale | string | yes | IETF | ภาษา |
| kycStatus | string | yes | none|basic|full | สถานะ KYC |
| createdAt | string | yes | ISO8601 | วันที่สร้าง |
| updatedAt | string | yes | ISO8601 | วันที่อัปเดต |
| traceid | string | yes | GUID | trace |



### PUT /xapi/v1/customers/me/settings

อัปเดตการตั้งค่าส่วนบุคคล

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| locale | string | no | IETF | ภาษา |
| theme | string | no | light|dark | ธีม |
| privacy | object | no | - | เช่น shareAnalytics: boolean |



**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| CUST-VAL-001 | 400 | รูปแบบค่าตั้งค่าไม่ถูกต้อง | validation fail | No |
| CUST-AUTH-001 | 403 | ไม่มีสิทธิ์แก้ไข | role/policy | No |



## Non-Functional Requirements

- **Latency**: p95 ≤ 200–400ms ตามประเภท (read/write)
- **Scalability**: รองรับ horizontal scaling, stateless, connection pooling
- **Observability**: Elastic APM, log โครงสร้าง, trace-span ผูกกับ `xapp-trace-id`, metrics ต่อ endpoint
- **Security**: TLS 1.2+, JWT + scopes, input validation, output encoding, PII masking ใน logs
- **Idempotency**: รองรับสำหรับคำสั่งที่สำคัญ (โอน/ถอน/แจ้งเตือน/ปล่อยเงิน)
- **Resilience**: circuit breaker, retry with backoff, DLQ สำหรับงาน async, graceful degradation
- **Caching**: ETag/Cache-Control, response caching (ที่เหมาะสม)
- **Compliance**: audit trail, data retention policy, consent/opt-in (สำหรับ marketing)

