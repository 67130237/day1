# Feature Flow: Profile, Banks & Documents

> This document maps the user journey to the related APIs (OpenAPI 3.1.0 spec from previous package). All paths are under base `/api/v1`.

## Flow Overview
```mermaid
flowchart TD
  A[Profile] --> B[GET /me]
  B --> C[Update -> PUT /me]
  B --> D[Banks -> GET /me/banks]
  D --> E[Add -> POST /me/banks]
  D --> F[Delete -> DELETE /me/banks/{id}]
  B --> G[Change PIN -> PUT /me/security/pin]
  B --> H[Reset Password -> POST /me/security/reset-password]
  B --> I[Preferences -> PUT /me/preferences]
  B --> J[Documents -> GET /documents]
```

## Step ↔ API Mapping
- Get/Update profile → `GET /me`, `PUT /me`
- Manage banks → `GET /me/banks`, `POST /me/banks`, `DELETE /me/banks/{id}`
- Security → `PUT /me/security/pin`, `POST /me/security/reset-password`
- Preferences → `PUT /me/preferences`
- Documents (receipts/contracts) → `GET /documents`


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
