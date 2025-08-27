# disbursement-service @ 1.0.0

## Overview

ปล่อยเงินกู้/คืนเงิน/cashback เชื่อมต่อระบบ downstream และบัญชีผู้รับ

## Headers


- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


## Endpoints


### POST /papi/v1/disbursements/eligibility

ตรวจคุณสมบัติเบื้องต้น

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| customerId | string | yes | - | ผู้ขอ |
| productCode | string | yes | - | ประเภทสินเชื่อ/รายการ |
| amount | string | yes | decimal | จำนวนเงินที่ขอ |



**Response**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| eligible | boolean | yes | - | ผ่าน/ไม่ผ่าน |
| maxOffer | string | no | decimal | เพดานเสนอ |
| reasons | array[string] | no | - | เหตุผลถ้าไม่ผ่าน |
| traceid | string | yes | GUID | trace |



**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| DISB-VAL-001 | 400 | ข้อมูลไม่ครบ | validation | No |
| DISB-DB-001 | 404 | ไม่พบลูกค้า | customer not found | No |



### POST /papi/v1/disbursements

เริ่มปล่อยเงิน

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| contractId | string | yes | - | สัญญาที่ลงนามแล้ว |
| payToAccountId | string | yes | - | บัญชีรับเงิน |
| amount | string | yes | decimal | จำนวนเงิน |
| currency | string | yes | ISO 4217 | สกุลเงิน |



**Response 201**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| disbursementId | string | yes | - | รหัส |
| status | string | yes | queued|processing|completed|failed | สถานะ |
| traceid | string | yes | GUID | trace |



**Business Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| DISB-VAL-002 | 409 | สัญญาไม่พร้อม/ยังไม่ลงนาม | contract status invalid | No |
| DISB-VAL-003 | 409 | บัญชีรับเงินไม่ถูกต้อง | payee invalid | No |
| DISB-EXT-001 | 502 | ระบบปลายทางล่ม | downstream down | Yes |
| DISB-VAL-004 | 409 | คำขอซ้ำ | Idempotency-Key | No |



## Non-Functional Requirements

- **Latency**: p95 ≤ 200–400ms ตามประเภท (read/write)
- **Scalability**: รองรับ horizontal scaling, stateless, connection pooling
- **Observability**: Elastic APM, log โครงสร้าง, trace-span ผูกกับ `xapp-trace-id`, metrics ต่อ endpoint
- **Security**: TLS 1.2+, JWT + scopes, input validation, output encoding, PII masking ใน logs
- **Idempotency**: รองรับสำหรับคำสั่งที่สำคัญ (โอน/ถอน/แจ้งเตือน/ปล่อยเงิน)
- **Resilience**: circuit breaker, retry with backoff, DLQ สำหรับงาน async, graceful degradation
- **Caching**: ETag/Cache-Control, response caching (ที่เหมาะสม)
- **Compliance**: audit trail, data retention policy, consent/opt-in (สำหรับ marketing)

