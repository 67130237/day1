# Feature Flow: Contract eSign & Disbursement

> This document maps the user journey to the related APIs (OpenAPI 3.1.0 spec from previous package). All paths are under base `/api/v1`.

## Flow Overview
```mermaid
sequenceDiagram
  participant App
  participant API
  participant eSign
  participant Bank
  App->>API: GET /contracts/{contractId}
  App->>API: POST /contracts/{contractId}/esign
  API->>eSign: eSign request
  eSign-->>API: signed
  API-->>App: 200 status=signed
  API->>Bank: disbursement
  Bank-->>API: sent
  App->>API: GET /disbursements/{contractId}
  API-->>App: 200 status=sent
```

## Step ↔ API Mapping
- Contract detail/PDF → `GET /contracts/{contractId}` / `GET /contracts/{contractId}/pdf`
- eSign → `POST /contracts/{contractId}/esign` (2FA before sign in UI)
- Disbursement status → `GET /disbursements/{contractId}`


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
