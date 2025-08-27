# accounts-process-api @ 1.0.0

## Overview
list + detail (balance + transactions)

## Global Headers (ตาม GlobalContract)
- `xapp-trace-id` (required)
- `Authorization: Bearer <JWT>` (ตามสิทธิ์ของแต่ละ endpoint)
- `Idempotency-Key` (สำหรับคำสั่งแบบ write)
- `Accept-Language` (optional)

**Response Header:** `xapp-source: accounts-process-api@1.0.0`  
**Error Envelope:** `{ "code": "STRING", "message": "STRING", "traceid": "GUID" }`

## Endpoints
### GET /v1/process/accounts/list → account-service GET /xapi/v1/accounts
### GET /v1/process/accounts/detail → account-service GET /xapi/v1/accounts/{accountId}/balance + /transactions
