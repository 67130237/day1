# account-service @ 1.0.0

## Overview

แสดงบัญชีของผู้ใช้, รายละเอียดบัญชี, ยอดคงเหลือ, ขีดจำกัด

## Headers


- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


## Endpoints


### GET /xapi/v1/accounts

รายการบัญชีของผู้ใช้ (pagination)

**Query**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| cursor | string | no | opaque | ตำแหน่งอ่านถัดไป |
| limit | number | no | 1..100 | จำนวนต่อหน้า |



**Response 200**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| items | array[Account] | yes | - | รายการบัญชี |
| nextCursor | string | no | opaque | ต่อหน้า |
| traceid | string | yes | GUID | trace |



**Account object**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| accountId | string | yes | - | หมายเลขภายใน |
| accountNo | string | yes | - | เลขบัญชีแสดงผล (mask) |
| type | string | yes | SAV|CUR|LOAN | ประเภท |
| currency | string | yes | ISO 4217 | สกุลเงิน |
| status | string | yes | active|closed|frozen | สถานะ |



### GET /xapi/v1/accounts/{accountId}/balance

ยอดคงเหลือ

**Response 200**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| available | string | yes | decimal string | ยอดใช้ได้ |
| ledger | string | yes | decimal string | ยอดบัญชี |
| currency | string | yes | ISO 4217 | สกุลเงิน |
| asOf | string | yes | ISO8601 | เวลาที่อัปเดต |
| traceid | string | yes | GUID | trace |



### GET /xapi/v1/accounts/{accountId}/transactions

ดึงรายการเดินบัญชี

**Query**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| from | string | no | ISO8601 | วันที่เริ่ม |
| to | string | no | ISO8601 | วันที่สิ้นสุด |
| cursor | string | no | opaque | ต่อหน้า |
| limit | number | no | 1..200 | จำนวนต่อหน้า |
| type | string | no | credit|debit | กรองตามทิศทาง |



**Item**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| txId | string | yes | - | รหัสรายการ |
| postedAt | string | yes | ISO8601 | เวลาลงบัญชี |
| amount | string | yes | decimal | จำนวนเงิน |
| currency | string | yes | ISO 4217 | สกุลเงิน |
| direction | string | yes | credit|debit | ทิศทาง |
| description | string | no | - | คำอธิบาย |
| balanceAfter | string | no | decimal | ยอดหลังทำรายการ |



**Business Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| ACC-DB-001 | 404 | ไม่พบบัญชี | accountId ไม่ถูกต้อง/ไม่เป็นของผู้ใช้ | No |
| ACC-VAL-001 | 400 | ช่วงวันที่ยาวเกินกำหนด | > 93 วัน | No |



## Non-Functional Requirements

- **Latency**: p95 ≤ 200–400ms ตามประเภท (read/write)
- **Scalability**: รองรับ horizontal scaling, stateless, connection pooling
- **Observability**: Elastic APM, log โครงสร้าง, trace-span ผูกกับ `xapp-trace-id`, metrics ต่อ endpoint
- **Security**: TLS 1.2+, JWT + scopes, input validation, output encoding, PII masking ใน logs
- **Idempotency**: รองรับสำหรับคำสั่งที่สำคัญ (โอน/ถอน/แจ้งเตือน/ปล่อยเงิน)
- **Resilience**: circuit breaker, retry with backoff, DLQ สำหรับงาน async, graceful degradation
- **Caching**: ETag/Cache-Control, response caching (ที่เหมาะสม)
- **Compliance**: audit trail, data retention policy, consent/opt-in (สำหรับ marketing)

