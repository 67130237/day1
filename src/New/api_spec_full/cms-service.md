# cms-service @ 1.0.0

## Overview

จัดการเนื้อหา/คอนฟิกสำหรับหน้าแอป (banner, cards, notices) พร้อมรองรับ publish workflow

## Headers


- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


## Endpoints


### GET /xapi/v1/cms/home

คอนฟิกหน้าแรก (public)

**Response**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| version | string | yes | - | เวอร์ชันคอนฟิก |
| widgets | array[Widget] | yes | - | องค์ประกอบหน้า |
| traceid | string | yes | GUID | trace |



**Widget object**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| type | string | yes | banner|card-list|notice | ชนิด |
| data | object | yes | - | ข้อมูลตามชนิด |
| ttlSeconds | number | no | - | อายุ cache |



### GET /xapi/v1/cms/banners?position=

ดึงแบนเนอร์ตามตำแหน่ง

**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| CMS-DB-001 | 404 | ไม่พบบทความ/แบนเนอร์ | slug/position not found | No |



## Non-Functional Requirements

- CDN + Cache-Control/ETag
- Draft/Publish/Versioning + rollback
- ขนาดสื่อจำกัด (เช่น 5MB ภาพเดี่ยว)
- Security สำหรับ backoffice (role-based)

- **Latency**: p95 ≤ 200–400ms ตามประเภท (read/write)
- **Scalability**: รองรับ horizontal scaling, stateless, connection pooling
- **Observability**: Elastic APM, log โครงสร้าง, trace-span ผูกกับ `xapp-trace-id`, metrics ต่อ endpoint
- **Security**: TLS 1.2+, JWT + scopes, input validation, output encoding, PII masking ใน logs
- **Idempotency**: รองรับสำหรับคำสั่งที่สำคัญ (โอน/ถอน/แจ้งเตือน/ปล่อยเงิน)
- **Resilience**: circuit breaker, retry with backoff, DLQ สำหรับงาน async, graceful degradation
- **Caching**: ETag/Cache-Control, response caching (ที่เหมาะสม)
- **Compliance**: audit trail, data retention policy, consent/opt-in (สำหรับ marketing)

