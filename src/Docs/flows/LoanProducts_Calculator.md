# Feature Flow: Loan Products & Calculator

> This document maps the user journey to the related APIs (OpenAPI 3.1.0 spec from previous package). All paths are under base `/api/v1`.

## Flow Overview
```mermaid
sequenceDiagram
  participant App
  participant API
  App->>API: GET /loan-products
  API-->>App: 200 products[]
  App->>API: GET /loan-products/{id}
  API-->>App: 200 product
  App->>API: POST /loan-calculator (amount, tenor, rateType)
  API-->>App: 200 monthlyPayment, totalInterest
```

## Step ↔ API Mapping
- List products → `GET /loan-products`
- Product detail → `GET /loan-products/{id}`
- Calculate installment → `POST /loan-calculator`


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
