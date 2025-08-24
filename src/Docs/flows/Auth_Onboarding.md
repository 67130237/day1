# Feature Flow: Auth & Onboarding

> This document maps the user journey to the related APIs (OpenAPI 3.1.0 spec from previous package). All paths are under base `/api/v1`.

## Flow Overview
```mermaid
flowchart TD
  A[Open App] --> B[Register or Login]
  B -->|Register| C[POST /auth/register]
  C --> D[POST /auth/otp/send -> /auth/otp/verify]
  D --> E[Login -> POST /auth/login]
  B -->|Login| E
  E --> F[POST /auth/biometric/link (optional)]
  F --> G[GET /auth/devices]
```

## Step ↔ API Mapping
1. **Register** → `POST /auth/register` → `201/200`
2. **Send OTP** → `POST /auth/otp/send` → `200`
3. **Verify OTP** → `POST /auth/otp/verify` → `200` (`verified=true`)
4. **Login** → `POST /auth/login` → returns `accessToken`, `refreshToken`
5. **Bind Biometric** → `POST /auth/biometric/link` (device binding)
6. **List Devices** → `GET /auth/devices`, **Revoke** → `DELETE /auth/devices/{deviceId}`
7. **Refresh Token** → `POST /auth/refresh`
8. **Logout** → `POST /auth/logout`


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
