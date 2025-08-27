# transaction-service @ 1.0.0

## Overview

ทำธุรกรรมโอนเงิน/ถอนเงิน และประวัติธุรกรรมที่เป็น execution (ต่างจากบัญชีที่เป็น statement)

## Headers


- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


## Endpoints — Transfers


### POST /xapi/v1/transfers/quote

คำนวณค่าธรรมเนียม/limit/AML pre-check

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| sourceAccountId | string | yes | - | บัญชีต้นทาง |
| destType | string | yes | internal|domestic|interbank | ประเภทปลายทาง |
| dest | object | yes | - | ปลายทาง (เช่น accountNo/bankCode) |
| amount | string | yes | decimal | จำนวนเงิน |
| currency | string | yes | ISO 4217 | สกุลเงิน |



**Response**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| fee | string | yes | decimal | ค่าธรรมเนียม |
| limitOk | boolean | yes | - | ผ่าน limit หรือไม่ |
| amlFlag | boolean | yes | - | พบสัญญาณเสี่ยงหรือไม่ |
| message | string | no | - | ข้อความแนะนำ |
| traceid | string | yes | GUID | trace |



**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| TXN-VAL-001 | 400 | จำนวนเงินไม่ถูกต้อง | <=0 หรือรูปแบบผิด | No |
| TXN-VAL-002 | 400 | บัญชีปลายทางไม่รองรับ | dest type not allowed | No |



### POST /xapi/v1/transfers

สร้างคำสั่งโอน (ใช้ `Idempotency-Key`)

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| sourceAccountId | string | yes | - | บัญชีต้นทาง |
| dest | object | yes | - | ปลายทาง (accountNo, bankCode, name) |
| amount | string | yes | decimal | จำนวนเงิน |
| currency | string | yes | ISO 4217 | สกุลเงิน |
| note | string | no | <=140 | หมายเหตุ |
| pinOrOtp | string | yes | - | ยืนยันความเป็นเจ้าของ (ตาม policy) |



**Response 201**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| transferId | string | yes | - | รหัสธุรกรรม |
| status | string | yes | queued|sent|settled|failed | สถานะ |
| expectedSettleAt | string | no | ISO8601 | เวลาคาดหมาย |
| traceid | string | yes | GUID | trace |



**Business Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| TXN-AUTH-003 | 401 | รหัสยืนยันไม่ถูกต้อง | PIN/OTP ผิด | No |
| TXN-DB-001 | 404 | ไม่พบบัญชีต้นทาง | sourceAccountId ผิด/ไม่เป็นของผู้ใช้ | No |
| TXN-VAL-003 | 409 | เกินวงเงินต่อวัน | daily limit exceeded | No |
| TXN-VAL-004 | 409 | ยอดเงินไม่พอ | insufficient balance | No |
| TXN-EXT-001 | 502 | ปลายทางล่ม | switch/bank down | Yes |
| TXN-VAL-005 | 409 | คำขอโอนซ้ำ | ชน Idempotency-Key | No |
| TXN-VAL-006 | 409 | นอกช่วงเวลาทำการ | cutoff window | No |
| TXN-SYS-001 | 500 | ระบบขัดข้อง | internal | Yes |



### GET /xapi/v1/transfers/{transferId}

ตรวจสถานะโอน

**Response**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| transferId | string | yes | - | รหัสรายการ |
| status | string | yes | queued|sent|settled|failed|reversed | สถานะ |
| fee | string | no | decimal | ค่าธรรมเนียม |
| failedReason | string | no | - | สาเหตุถ้าล้มเหลว |
| traceid | string | yes | GUID | trace |



## Endpoints — Withdrawals


### POST /xapi/v1/withdrawals

ร้องขอถอน (เช่น ATM code หรือ counter)

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| sourceAccountId | string | yes | - | บัญชีต้นทาง |
| channel | string | yes | atm|counter | ช่องทาง |
| amount | string | yes | decimal | จำนวนเงิน |
| currency | string | yes | ISO 4217 | สกุลเงิน |



**Response 201**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| withdrawalId | string | yes | - | รหัสถอน |
| status | string | yes | issued|redeemed|expired|canceled | สถานะ |
| token | string | no | - | รหัสถอน (ถ้ามี) |
| expireAt | string | no | ISO8601 | หมดอายุ |
| traceid | string | yes | GUID | trace |



**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| WDR-VAL-001 | 409 | ยอดเงินไม่พอ | insufficient balance | No |
| WDR-VAL-002 | 409 | เกินวงเงินถอน | daily limit exceeded | No |
| WDR-EXT-001 | 502 | ช่องทางถอนล่ม | atm/counter down | Yes |
| WDR-VAL-003 | 409 | คำขอซ้ำ | Idempotency-Key | No |



## Non-Functional Requirements

- **Latency**: p95 ≤ 200–400ms ตามประเภท (read/write)
- **Scalability**: รองรับ horizontal scaling, stateless, connection pooling
- **Observability**: Elastic APM, log โครงสร้าง, trace-span ผูกกับ `xapp-trace-id`, metrics ต่อ endpoint
- **Security**: TLS 1.2+, JWT + scopes, input validation, output encoding, PII masking ใน logs
- **Idempotency**: รองรับสำหรับคำสั่งที่สำคัญ (โอน/ถอน/แจ้งเตือน/ปล่อยเงิน)
- **Resilience**: circuit breaker, retry with backoff, DLQ สำหรับงาน async, graceful degradation
- **Caching**: ETag/Cache-Control, response caching (ที่เหมาะสม)
- **Compliance**: audit trail, data retention policy, consent/opt-in (สำหรับ marketing)

