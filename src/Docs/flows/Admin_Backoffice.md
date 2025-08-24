# Feature Flow: Admin / Backoffice

> This document maps the user journey to the related APIs (OpenAPI 3.1.0 spec from previous package). All paths are under base `/api/v1`.

## Flow Overview
```mermaid
flowchart TD
  A[Review Queue] --> B[GET /admin/applications?status=pending]
  B --> C[Approve -> POST /admin/applications/{appId}/approve]
  B --> D[Reject -> POST /admin/applications/{appId}/reject]
  B --> E[Offer -> POST /admin/applications/{appId}/offer]
  E --> F[Disburse -> POST /admin/loans/{loanId}/disburse]
  F --> G[Collections -> POST /admin/loans/{loanId}/remind]
  F --> H[Writeoff -> POST /admin/loans/{loanId}/writeoff]
```

## Step ↔ API Mapping
- Review queue → `GET /admin/applications?status=pending`
- Decisions → `POST /admin/applications/{appId}/approve|reject`
- Price/Offer → `POST /admin/applications/{appId}/offer`
- Disbursement → `POST /admin/loans/{loanId}/disburse`
- Collections/Writeoff → `POST /admin/loans/{loanId}/remind`, `POST /admin/loans/{loanId}/writeoff`


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
