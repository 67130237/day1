# kyc-process-api @ 1.0.0

## Overview
verify-dopa via identity-service

## Global Headers (ตาม GlobalContract)
- `xapp-trace-id` (required)
- `Authorization: Bearer <JWT>` (ตามสิทธิ์ของแต่ละ endpoint)
- `Idempotency-Key` (สำหรับคำสั่งแบบ write)
- `Accept-Language` (optional)

**Response Header:** `xapp-source: kyc-process-api@1.0.0`  
**Error Envelope:** `{ "code": "STRING", "message": "STRING", "traceid": "GUID" }`

## Endpoint
### POST /v1/process/kyc/verify → identity-service POST /papi/v1/identity/verify-dopa (→ dopa-service)
