# register-process-api @ 1.0.0

## Overview
Orchestrate สมัครผู้ใช้: register → OTP → DOPA → activate → Welcome Push

## Global Headers (ตาม GlobalContract)
- `xapp-trace-id` (required)
- `Authorization: Bearer <JWT>` (ตามสิทธิ์ของแต่ละ endpoint)
- `Idempotency-Key` (สำหรับคำสั่งแบบ write)
- `Accept-Language` (optional)

**Response Header:** `xapp-source: register-process-api@1.0.0`  
**Error Envelope:** `{ "code": "STRING", "message": "STRING", "traceid": "GUID" }`

## Endpoints
### POST /v1/process/register/init
- call: auth-service POST /xapi/v1/auth/register
- call: otp-service POST /sapi/v1/otp/send

### POST /v1/process/register/verify-otp
- call: otp-service POST /sapi/v1/otp/verify

### POST /v1/process/register/verify-dopa
- call: identity-service POST /papi/v1/identity/verify-dopa → dopa-service POST /sapi/v1/dopa/verify

### POST /v1/process/register/activate
- call: identity-service POST /papi/v1/identity
- call: customer-service GET /xapi/v1/customers/me
- call: account-service GET /xapi/v1/accounts
- call: notification-service POST /papi/v1/notifications
