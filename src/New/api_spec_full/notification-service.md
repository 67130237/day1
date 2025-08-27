# notification-service @ 1.0.0

## Overview

ส่งการแจ้งเตือนหลายช่องทาง (push, in-app, email, sms, line), inbox ผู้ใช้, preference, schedule

## Headers


- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


## Endpoints


### POST /papi/v1/notifications

สร้างคำสั่งแจ้งเตือน (รองรับ schedule และ idempotency)

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| messageId | string | no | GUID | ถ้าไม่ส่ง ระบบจะสร้างให้ |
| topic | string | yes | - | หัวข้อ เช่น payment.success |
| title | string | yes | <=120 | หัวข้อข้อความ |
| body | string | yes | <=1024 | เนื้อหา |
| data | object | no | - | payload เสริม |
| audience | object | yes | - | ผู้รับ: type=userIds|segment|broadcast |
| channels | array[string] | yes | push|inapp|email|sms|line | ช่องทาง |
| priority | string | no | low|normal|high | ความสำคัญ |
| scheduleAt | string | no | ISO8601 | เวลาส่ง |
| template | object | no | - | key, locale, vars |
| idempotencyKey | string | yes | - | ป้องกันส่งซ้ำ |



**Response 202**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| messageId | string | yes | - | รหัสข้อความ |
| status | string | yes | queued | สถานะ |
| traceid | string | yes | GUID | trace |



**Business Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| NOTI-VAL-001 | 400 | payload ไม่ถูกต้อง | validation fail | No |
| NOTI-VAL-002 | 409 | ข้อความซ้ำ | ชน idempotencyKey | No |
| NOTI-VAL-003 | 409 | ตั้งเวลาในอดีต | schedule in past | No |
| NOTI-AUTH-001 | 403 | ไม่มีสิทธิ์ส่งหัวข้อนี้ | policy | No |
| NOTI-EXT-001 | 502 | ผู้ให้บริการปลายทางล่ม | provider down | Yes |
| NOTI-SYS-001 | 500 | ระบบขัดข้อง | internal | Yes |



### GET /xapi/v1/inbox?unreadOnly=&limit=&cursor=

อ่านกล่องข้อความของผู้ใช้

**Item**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| id | string | yes | - | รหัส |
| topic | string | yes | - | หัวข้อ |
| title | string | yes | - | หัวข้อ |
| body | string | yes | - | เนื้อหา |
| createdAt | string | yes | ISO8601 | เวลา |
| read | boolean | yes | - | อ่านแล้วหรือไม่ |



### POST /xapi/v1/inbox/{id}/read

มาร์คว่าอ่านแล้ว

### GET /xapi/v1/preferences/me

อ่าน preference

**Preference**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| muteAll | boolean | yes | - | ปิดทั้งหมด |
| channels.push.enabled | boolean | yes | - | เปิด push |
| channels.email.enabled | boolean | yes | - | เปิด email |
| topics.payment.* | boolean | no | - | ตัวอย่างกำหนดรายหัวข้อ |



## Non-Functional Requirements

- Throughput ≥ 1k msg/min, DLQ & replay
- Quiet hours, consent (transactional vs marketing)
- Provider adapters pluggable (FCM/APNs/SMS/Email/LINE)
- **Latency**: p95 ≤ 200–400ms ตามประเภท (read/write)
- **Scalability**: รองรับ horizontal scaling, stateless, connection pooling
- **Observability**: Elastic APM, log โครงสร้าง, trace-span ผูกกับ `xapp-trace-id`, metrics ต่อ endpoint
- **Security**: TLS 1.2+, JWT + scopes, input validation, output encoding, PII masking ใน logs
- **Idempotency**: รองรับสำหรับคำสั่งที่สำคัญ (โอน/ถอน/แจ้งเตือน/ปล่อยเงิน)
- **Resilience**: circuit breaker, retry with backoff, DLQ สำหรับงาน async, graceful degradation
- **Caching**: ETag/Cache-Control, response caching (ที่เหมาะสม)
- **Compliance**: audit trail, data retention policy, consent/opt-in (สำหรับ marketing)

