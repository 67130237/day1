# login-process-api @ 1.0.0

## Overview
Orchestrate ล็อกอิน PIN/Biometric + OTP (ถ้าจำเป็น) + optional Sign-in Push

## Global Headers (ตาม GlobalContract)
- `xapp-trace-id` (required)
- `Authorization: Bearer <JWT>` (ตามสิทธิ์ของแต่ละ endpoint)
- `Idempotency-Key` (สำหรับคำสั่งแบบ write)
- `Accept-Language` (optional)

**Response Header:** `xapp-source: login-process-api@1.0.0`  
**Error Envelope:** `{ "code": "STRING", "message": "STRING", "traceid": "GUID" }`

## Endpoints
### POST /v1/process/login/pin/init → auth-service /xapi/v1/auth/login/pin
### POST /v1/process/login/pin/request-otp → otp-service /sapi/v1/otp/send
### POST /v1/process/login/pin/confirm → otp-service /sapi/v1/otp/verify + notification-service /papi/v1/notifications
### POST /v1/process/login/biometric/verify → auth-service /xapi/v1/auth/login/biometric
