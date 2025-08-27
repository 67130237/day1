# withdraw-process-api @ 1.0.0

## Overview
prepare → confirm

## Global Headers (ตาม GlobalContract)
- `xapp-trace-id` (required)
- `Authorization: Bearer <JWT>` (ตามสิทธิ์ของแต่ละ endpoint)
- `Idempotency-Key` (สำหรับคำสั่งแบบ write)
- `Accept-Language` (optional)

**Response Header:** `xapp-source: withdraw-process-api@1.0.0`  
**Error Envelope:** `{ "code": "STRING", "message": "STRING", "traceid": "GUID" }`

## Endpoints
### POST /v1/process/withdraw/prepare (local validation)
### POST /v1/process/withdraw/confirm → transaction-service POST /xapi/v1/withdrawals + notification-service POST /papi/v1/notifications
