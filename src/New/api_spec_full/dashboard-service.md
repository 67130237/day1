# dashboard-service @ 1.0.0

## Overview

รวมข้อมูลหน้าแรกของแอปจากหลายบริการ (accounts, transactions, cms, notifications)

## Headers


- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


## GET /xapi/v1/dashboard

รวม widget และสรุป

**Response 200**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| balances | array[BalanceSummary] | yes | - | ยอดคงเหลือย่อ |
| recentTransactions | array[TxSummary] | no | - | ล่าสุด |
| banners | array[Banner] | no | - | แบนเนอร์ |
| unreadCount | number | no | - | แจ้งเตือนไม่ได้อ่าน |
| traceid | string | yes | GUID | trace |



**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| DASH-EXT-001 | 502 | บริการต้นทางบางตัวไม่พร้อม | partial failure | Yes |
| DASH-SYS-001 | 500 | ระบบขัดข้อง | internal | Yes |



## Non-Functional Requirements

- Fan-out + parallel calls + time budget per upstream
- Fallback cache สำหรับ widget สำคัญ
- **Latency**: p95 ≤ 200–400ms ตามประเภท (read/write)
- **Scalability**: รองรับ horizontal scaling, stateless, connection pooling
- **Observability**: Elastic APM, log โครงสร้าง, trace-span ผูกกับ `xapp-trace-id`, metrics ต่อ endpoint
- **Security**: TLS 1.2+, JWT + scopes, input validation, output encoding, PII masking ใน logs
- **Idempotency**: รองรับสำหรับคำสั่งที่สำคัญ (โอน/ถอน/แจ้งเตือน/ปล่อยเงิน)
- **Resilience**: circuit breaker, retry with backoff, DLQ สำหรับงาน async, graceful degradation
- **Caching**: ETag/Cache-Control, response caching (ที่เหมาะสม)
- **Compliance**: audit trail, data retention policy, consent/opt-in (สำหรับ marketing)

