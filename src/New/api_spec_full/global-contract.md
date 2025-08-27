# Global API Contract

**Date:** 2025-08-27


- **Request Headers**
  - `xapp-trace-id` (string, GUID) — **required**. ใช้สำหรับ trace propagation
  - `Authorization: Bearer <JWT>` — **required** สำหรับ endpoint ที่ต้องการสิทธิ์
  - `Idempotency-Key` (string) — **required** สำหรับคำสั่งแบบ write ที่ต้องการ exactly-once (เช่น transfer, notify)
  - `Accept-Language` (string, e.g., `th-TH`, `en-US`) — optional, สำหรับ localized message

- **Response Headers**
  - `xapp-source` = `<service>@<version>` (เช่น `accounts-sapi@1.0.0`)


ทุกบริการต้องตอบ Error Envelope ตาม GlobalContract:

```json
{
  "code": "STRING",
  "message": "HUMAN_READABLE_MESSAGE",
  "traceid": "GUID"
}
```


**Error Code Format**: `[SERVICE]-[CATEGORY]-[NUMBER]`  
`CATEGORY`: VAL (Validation) · AUTH (AuthZ/AuthN) · DB (Data) · EXT (External) · SYS (Internal)

**HTTP Mapping**: 400→VAL · 401/403→AUTH · 404→DB · 409→VAL · 500→SYS · 502/503/504→EXT

