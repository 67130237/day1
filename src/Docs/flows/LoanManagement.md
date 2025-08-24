# Feature Flow: Loan Management

> This document maps the user journey to the related APIs (OpenAPI 3.1.0 spec from previous package). All paths are under base `/api/v1`.

## Flow Overview
```mermaid
flowchart TD
  A[Open My Loans] --> B[GET /loans]
  B --> C[GET /loans/{loanId}]
  C --> D[GET /loans/{loanId}/schedule]
  D --> E[GET /loans/{loanId}/transactions]
  C --> F[POST /loans/{loanId}/early-quote]
  C --> G[POST /loans/{loanId}/partial-quote]
  C --> H[POST /loans/{loanId}/restructure/request]
```

## Step ↔ API Mapping
- List loans → `GET /loans`
- Loan detail → `GET /loans/{loanId}`
- Repayment schedule → `GET /loans/{loanId}/schedule`
- Transactions → `GET /loans/{loanId}/transactions`
- Early settlement quote → `POST /loans/{loanId}/early-quote`
- Partial settlement quote → `POST /loans/{loanId}/partial-quote`
- Restructure request → `POST /loans/{loanId}/restructure/request` → `202 Accepted`


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
