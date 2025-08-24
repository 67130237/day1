# Feature Flow: Payments & Billing

> This document maps the user journey to the related APIs (OpenAPI 3.1.0 spec from previous package). All paths are under base `/api/v1`.

## Flow Overview
```mermaid
flowchart TD
  A[Due Reminder] --> B[GET /billing/{loanId}/next-due]
  B --> C[GET /billing/{loanId}/invoices]
  C --> D[GET /billing/invoices/{invoiceId}]
  D --> E[POST /payments/intent]
  E --> F[Complete via PSP (QR/Card)]
  F --> G[PSP -> POST /webhooks/payments/provider]
  G --> H[GET /payments/{paymentId}]
  H --> I[GET /payments/{paymentId}/receipt]
```

## Step ↔ API Mapping
- Next due → `GET /billing/{loanId}/next-due`
- Invoices list/detail → `GET /billing/{loanId}/invoices`, `GET /billing/invoices/{invoiceId}`
- Create payment intent → `POST /payments/intent` (**Idempotency-Key** recommended for `charge`)
- Direct charge (non-redirect) → `POST /payments/charge`
- Payment status → `GET /payments/{paymentId}`
- Receipt (PDF/URL) → `GET /payments/{paymentId}/receipt`
- PSP webhook → `POST /webhooks/payments/provider`
- Autodebit setup/status/cancel → `POST /payments/autodebit/setup`, `GET /payments/autodebit/status`, `DELETE /payments/autodebit`


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
