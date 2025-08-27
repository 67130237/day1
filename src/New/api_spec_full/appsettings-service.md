# appsettings-service @ 1.0.0

## Overview

ให้บริการอ่านค่า settings ระดับแอป (public) และภายใน (secured), พร้อม user overrides บางส่วน

## Headers


- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


## Endpoints


### GET /xapi/v1/appsettings?scope=public

คืนค่า public settings สำหรับ client

**Response**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| scope | string | yes | public | ขอบเขต |
| data | object | yes | - | key/value |
| etag | string | yes | - | ใช้กับ If-None-Match |
| traceid | string | yes | GUID | trace |



### GET /sapi/v1/appsettings/{key}

อ่านค่า (internal secured)

### PUT /sapi/v1/appsettings/{key}

อัปเดตค่า พร้อม validation

**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| APPSET-DB-001 | 404 | ไม่พบ key | missing | No |
| APPSET-VAL-001 | 400 | ค่าไม่ผ่าน schema | validation | No |
| APPSET-AUTH-001 | 403 | ไม่มีสิทธิ์ | forbidden | No |



## Non-Functional Requirements

- Strong validation + schema registry
- Rollout/rollback with version
- ETag/Cache และ rate-limit read
- **Latency**: p95 ≤ 200–400ms ตามประเภท (read/write)
- **Scalability**: รองรับ horizontal scaling, stateless, connection pooling
- **Observability**: Elastic APM, log โครงสร้าง, trace-span ผูกกับ `xapp-trace-id`, metrics ต่อ endpoint
- **Security**: TLS 1.2+, JWT + scopes, input validation, output encoding, PII masking ใน logs
- **Idempotency**: รองรับสำหรับคำสั่งที่สำคัญ (โอน/ถอน/แจ้งเตือน/ปล่อยเงิน)
- **Resilience**: circuit breaker, retry with backoff, DLQ สำหรับงาน async, graceful degradation
- **Caching**: ETag/Cache-Control, response caching (ที่เหมาะสม)
- **Compliance**: audit trail, data retention policy, consent/opt-in (สำหรับ marketing)

