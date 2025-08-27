# transfer-process-api @ 1.0.0

## Overview
prepare → request-otp → confirm

## Global Headers (ตาม GlobalContract)
- `xapp-trace-id` (required)
- `Authorization: Bearer <JWT>` (ตามสิทธิ์ของแต่ละ endpoint)
- `Idempotency-Key` (สำหรับคำสั่งแบบ write)
- `Accept-Language` (optional)

**Response Header:** `xapp-source: transfer-process-api@1.0.0`  
**Error Envelope:** `{ "code": "STRING", "message": "STRING", "traceid": "GUID" }`

## Endpoints
### POST /v1/process/transfer/prepare → account-service GET /xapi/v1/accounts/{accountId}/balance + transaction-service POST /xapi/v1/transfers/quote
### POST /v1/process/transfer/request-otp → otp-service POST /sapi/v1/otp/send
### POST /v1/process/transfer/confirm → otp-service POST /sapi/v1/otp/verify + transaction-service POST /xapi/v1/transfers + notification-service POST /papi/v1/notifications
