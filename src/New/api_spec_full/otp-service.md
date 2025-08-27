# otp-service @ 1.0.0

## Overview

ส่ง/ตรวจ OTP หลายช่องทาง พร้อม rate-limit, one-time use

## Headers


- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


## Endpoints


### POST /sapi/v1/otp/send


**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| target | string | yes | E.164|email | ปลายทาง |
| channel | string | yes | sms|email|push | ช่องทาง |
| purpose | string | yes | login|transfer|register|others | วัตถุประสงค์ |
| ttlSeconds | number | no | 60..600 | อายุ OTP |



**Response**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| otpId | string | yes | - | รหัสอ้างอิง OTP |
| expiresAt | string | yes | ISO8601 | หมดอายุ |
| traceid | string | yes | GUID | trace |



**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| OTP-VAL-001 | 400 | ช่องทาง/ปลายทางไม่รองรับ | validation | No |
| OTP-EXT-001 | 502 | ผู้ให้บริการส่ง OTP ล่ม | gateway down | Yes |
| OTP-AUTH-001 | 429 | ขอ OTP ถี่เกินไป | rate limit | Yes |



### POST /sapi/v1/otp/verify


**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| otpId | string | yes | - | รหัสอ้างอิง |
| code | string | yes | 6 digits | โค้ด |
| purpose | string | yes | login|transfer|register|others | ต้องตรงกับตอนส่ง |



**Response**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| valid | boolean | yes | - | ผลการตรวจ |
| traceid | string | yes | GUID | trace |



**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| OTP-VAL-002 | 400 | โค้ดไม่ถูกต้อง | invalid code | No |
| OTP-VAL-003 | 410 | OTP หมดอายุ/ถูกใช้ไปแล้ว | expired/used | No |



## Non-Functional Requirements

- **Latency**: p95 ≤ 200–400ms ตามประเภท (read/write)
- **Scalability**: รองรับ horizontal scaling, stateless, connection pooling
- **Observability**: Elastic APM, log โครงสร้าง, trace-span ผูกกับ `xapp-trace-id`, metrics ต่อ endpoint
- **Security**: TLS 1.2+, JWT + scopes, input validation, output encoding, PII masking ใน logs
- **Idempotency**: รองรับสำหรับคำสั่งที่สำคัญ (โอน/ถอน/แจ้งเตือน/ปล่อยเงิน)
- **Resilience**: circuit breaker, retry with backoff, DLQ สำหรับงาน async, graceful degradation
- **Caching**: ETag/Cache-Control, response caching (ที่เหมาะสม)
- **Compliance**: audit trail, data retention policy, consent/opt-in (สำหรับ marketing)

