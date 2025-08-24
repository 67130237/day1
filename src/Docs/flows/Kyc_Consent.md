# Feature Flow: eKYC & Credit Consent

> This document maps the user journey to the related APIs (OpenAPI 3.1.0 spec from previous package). All paths are under base `/api/v1`.

## Flow Overview
```mermaid
flowchart TD
  A[Start KYC] --> B[GET /kyc/profile]
  B --> C[POST /kyc/submit]
  C --> D[POST /kyc/documents]
  D --> E[POST /kyc/face/verify]
  E --> F[GET /kyc/check]
  F --> G[POST /consent/credit-check]
  G --> H[GET /consent/credit-check/status]
```

## Step ↔ API Mapping
- Check current status → `GET /kyc/profile`
- Submit KYC form → `POST /kyc/submit` → `202 Accepted`
- Upload documents → `POST /kyc/documents`
- Face/Liveness → `POST /kyc/face/verify` → status becomes `verified`
- Poll status → `GET /kyc/check`
- Credit consent → `POST /consent/credit-check`
- Credit status → `GET /consent/credit-check/status`


## Common HTTP Status Codes
- **200 OK** — success
- **201 Created** — resource created
- **202 Accepted** — async processing
- **400 Bad Request** — validation_error
- **401 Unauthorized** — missing/invalid token
- **403 Forbidden** — not allowed
- **404 Not Found** — resource not found
- **409 Conflict** — idempotency conflict / state conflict
- **422 Unprocessable Entity** — business rule violation
- **429 Too Many Requests** — rate limited
- **500 Internal Server Error** — server error


### Error Model
```json
{
  "error": "<code>",
  "message": "<human readable message>",
  "traceId": "00-<w3c-trace>-01"
}
```
