# dopa-service @ 1.0.0

## Overview

บริการตรวจข้อมูลบุคคลกับแหล่งข้อมูลภาครัฐ (DOPA)

## Headers


- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


## POST /sapi/v1/dopa/verify


**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| citizenId | string | yes | 13 digits | เลขประชาชน |
| firstName | string | yes | - | ชื่อ |
| lastName | string | yes | - | สกุล |
| birthDate | string | yes | YYYY-MM-DD | วันเกิด |
| laserCode | string | no | 12 chars | อาจจำเป็น |



**Response**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| verified | boolean | yes | - | ผ่านการตรวจ |
| matchedFields | array[string] | yes | - | ฟิลด์ที่ตรงกัน |
| referenceId | string | no | - | รหัสอ้างอิงภายนอก |
| traceid | string | yes | GUID | trace |



**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| DOPA-VAL-001 | 400 | รูปแบบข้อมูลไม่ถูกต้อง | validation | No |
| DOPA-DB-001 | 404 | ไม่พบข้อมูลบุคคล | record not found | No |
| DOPA-EXT-001 | 503 | บริการภายนอกไม่พร้อมใช้งาน | maintenance/timeout | Yes |
| DOPA-SYS-001 | 500 | ระบบขัดข้อง | internal error | Yes |



## Non-Functional Requirements

- **Latency**: p95 ≤ 200–400ms ตามประเภท (read/write)
- **Scalability**: รองรับ horizontal scaling, stateless, connection pooling
- **Observability**: Elastic APM, log โครงสร้าง, trace-span ผูกกับ `xapp-trace-id`, metrics ต่อ endpoint
- **Security**: TLS 1.2+, JWT + scopes, input validation, output encoding, PII masking ใน logs
- **Idempotency**: รองรับสำหรับคำสั่งที่สำคัญ (โอน/ถอน/แจ้งเตือน/ปล่อยเงิน)
- **Resilience**: circuit breaker, retry with backoff, DLQ สำหรับงาน async, graceful degradation
- **Caching**: ETag/Cache-Control, response caching (ที่เหมาะสม)
- **Compliance**: audit trail, data retention policy, consent/opt-in (สำหรับ marketing)

