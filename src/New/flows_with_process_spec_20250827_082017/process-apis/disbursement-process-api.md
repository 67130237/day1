# disbursement-process-api @ 1.0.0

## Overview
eligibility → start

## Global Headers (ตาม GlobalContract)
- `xapp-trace-id` (required)
- `Authorization: Bearer <JWT>` (ตามสิทธิ์ของแต่ละ endpoint)
- `Idempotency-Key` (สำหรับคำสั่งแบบ write)
- `Accept-Language` (optional)

**Response Header:** `xapp-source: disbursement-process-api@1.0.0`  
**Error Envelope:** `{ "code": "STRING", "message": "STRING", "traceid": "GUID" }`

## Endpoints
### POST /v1/process/disbursement/eligibility → disbursement-service POST /papi/v1/disbursements/eligibility
### POST /v1/process/disbursement/start → disbursement-service POST /papi/v1/disbursements + notification-service POST /papi/v1/notifications
