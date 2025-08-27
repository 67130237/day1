# bootstrap-process-api @ 1.0.0

## Overview
รวม appsettings (public) + โครงหน้าหลักจาก CMS

## Global Headers (ตาม GlobalContract)
- `xapp-trace-id` (required)
- `Authorization: Bearer <JWT>` (ตามสิทธิ์ของแต่ละ endpoint)
- `Idempotency-Key` (สำหรับคำสั่งแบบ write)
- `Accept-Language` (optional)

**Response Header:** `xapp-source: bootstrap-process-api@1.0.0`  
**Error Envelope:** `{ "code": "STRING", "message": "STRING", "traceid": "GUID" }`

## Endpoint
### GET /v1/process/bootstrap/init
- appsettings-service GET /xapi/v1/appsettings?scope=public
- cms-service GET /xapi/v1/cms/home
