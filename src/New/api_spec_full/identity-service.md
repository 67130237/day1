# identity-service @ 1.0.0

## Overview

จัดการตัวตนผู้ใช้ (Identity): create profile, verify citizen data (ผ่าน DOPA), PIN lifecycle, biometric enrol/verify (ฝั่งระบบ)

## Headers


- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


## Endpoints


### POST /papi/v1/identity

สร้าง identity record ภายในระบบ (post-registration)

**Security:** JWT (scope: `identity:write`)  
**Idempotency:** ใช้ business key: citizenId

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| citizenId | string | yes | 13 digits | เลขประชาชน |
| firstName | string | yes | - | ชื่อ |
| lastName | string | yes | - | สกุล |
| birthDate | string | yes | YYYY-MM-DD | วันเกิด |
| laserCode | string | no | 12 chars | เลข laser หลังบัตร |
| address | object | no | - | ที่อยู่ปัจจุบัน |
| metadata | object | no | - | ข้อมูลอื่นๆ |



**Response 201**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| identityId | string | yes | - | รหัส identity ภายใน |
| status | string | yes | pending|verified | สถานะ |
| traceid | string | yes | GUID | trace |



**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| IDEN-VAL-001 | 400 | ข้อมูลไม่ครบ/รูปแบบไม่ถูกต้อง | validation fail | No |
| IDEN-DB-001 | 409 | มี identity นี้แล้ว | duplicate citizenId | No |
| IDEN-SYS-001 | 500 | ระบบขัดข้อง | internal | Yes |



### POST /papi/v1/identity/verify-dopa

ยืนยันข้อมูลกับ DOPA

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| citizenId | string | yes | 13 digits | เลขประชาชน |
| firstName | string | yes | - | ชื่อ |
| lastName | string | yes | - | สกุล |
| birthDate | string | yes | YYYY-MM-DD | วันเกิด |
| laserCode | string | no | 12 chars | อาจจำเป็นตาม policy |



**Response**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| verified | boolean | yes | - | ผลการตรวจ |
| matchedFields | array[string] | yes | - | ฟิลด์ที่ตรงกัน |
| traceid | string | yes | GUID | trace |



**Errors**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| IDEN-EXT-001 | 502 | DOPA ไม่พร้อมใช้งาน | timeout/down | Yes |
| IDEN-VAL-002 | 400 | ข้อมูลไม่ตรงกับฐาน DOPA | mismatch | No |
| IDEN-DB-002 | 404 | ไม่พบใน DOPA | record not found | No |



### PUT /papi/v1/identity/pin

ตั้ง/เปลี่ยน PIN

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| oldPin | string | no | 6 digits | จำเป็นถ้ามี PIN เดิม |
| newPin | string | yes | 6 digits | PIN ใหม่ |
| confirmPin | string | yes | = newPin | ยืนยัน |



**Errors เพิ่มเติม**
| Code | HTTP | Message (TH) | Condition | Retry? |
| --- | --- | --- | --- | --- |
| IDEN-AUTH-001 | 401 | PIN เดิมไม่ถูกต้อง | oldPin mismatch | No |
| IDEN-VAL-003 | 400 | PIN ใหม่อ่อนแอ | common patterns | No |



### POST /papi/v1/identity/biometric/enrol

ลงทะเบียน biometric template (ฝั่งระบบ)

**Request**
| Field | Type | Required | Constraints | Description |
| --- | --- | --- | --- | --- |
| deviceId | string | yes | - | อุปกรณ์ |
| publicKey | string | yes | PEM | คีย์สาธารณะสำหรับ verify assertion |
| signature | string | yes | base64 | ลงนามจาก device |



## Non-Functional Requirements

- **Latency**: p95 ≤ 200–400ms ตามประเภท (read/write)
- **Scalability**: รองรับ horizontal scaling, stateless, connection pooling
- **Observability**: Elastic APM, log โครงสร้าง, trace-span ผูกกับ `xapp-trace-id`, metrics ต่อ endpoint
- **Security**: TLS 1.2+, JWT + scopes, input validation, output encoding, PII masking ใน logs
- **Idempotency**: รองรับสำหรับคำสั่งที่สำคัญ (โอน/ถอน/แจ้งเตือน/ปล่อยเงิน)
- **Resilience**: circuit breaker, retry with backoff, DLQ สำหรับงาน async, graceful degradation
- **Caching**: ETag/Cache-Control, response caching (ที่เหมาะสม)
- **Compliance**: audit trail, data retention policy, consent/opt-in (สำหรับ marketing)

