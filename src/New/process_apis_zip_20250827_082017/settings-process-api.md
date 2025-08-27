# settings-process-api @ 1.0.0

## Overview
bootstrap + update

## Global Headers (ตาม GlobalContract)
- `xapp-trace-id` (required)
- `Authorization: Bearer <JWT>` (ตามสิทธิ์ของแต่ละ endpoint)
- `Idempotency-Key` (สำหรับคำสั่งแบบ write)
- `Accept-Language` (optional)

**Response Header:** `xapp-source: settings-process-api@1.0.0`  
**Error Envelope:** `{ "code": "STRING", "message": "STRING", "traceid": "GUID" }`

## Endpoints
### GET /v1/process/settings/bootstrap → appsettings-service GET /xapi/v1/appsettings?scope=public
### PUT /v1/process/settings/update → customer-service PUT /xapi/v1/customers/me/settings
