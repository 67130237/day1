# Feature Flow: Loan Application

> This document maps the user journey to the related APIs (OpenAPI 3.1.0 spec from previous package). All paths are under base `/api/v1`.

## Flow Overview
```mermaid
flowchart TD
  A[Choose Product] --> B[POST /loan-applications]
  B --> C[GET /loan-applications/{appId}]
  C --> D[POST /loan-applications/{appId}/documents]
  D --> E[POST /loan-applications/{appId}/submit]
  E --> F[GET /loan-applications/{appId}/status]
  F --> G{approved?}
  G -- Yes --> H[POST /loan-applications/{appId}/accept-offer]
  G -- No --> I[POST /loan-applications/{appId}/reject-offer]
```

## Step ↔ API Mapping
- Create application (use **Idempotency-Key**) → `POST /loan-applications` → `201 Created`
- Get application → `GET /loan-applications/{appId}`
- Upload docs → `POST /loan-applications/{appId}/documents`
- Submit for review → `POST /loan-applications/{appId}/submit` → `202 Accepted`
- Track decision → `GET /loan-applications/{appId}/status`
- Accept/Reject offer → `POST /loan-applications/{appId}/accept-offer` / `reject-offer`

### Listing & Update
- List my applications → `GET /loan-applications?status=...`
- Update draft → `PUT /loan-applications/{appId}`


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
