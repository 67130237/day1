# GlobalContract @ 1.0.0

## Headers
- **Request**
  - `xapp-trace-id`: `string(UUID)` — required for all requests
  - `Authorization`: `Bearer <JWT>` — as required per endpoint
  - `Idempotency-Key`: `string` — required on idempotent write endpoints (e.g., POST transfers, withdrawals)
  - `Accept-Language`: `th-TH|en-US` — optional

- **Response**
  - `xapp-source`: `<service-name>@<version>`

## Error Envelope (ALL services)
```json
{ "code": "STRING", "message": "STRING", "traceId": "UUID", "details": { } }
```

## Common Status
- `200 OK` / `201 Created` / `202 Accepted`
- `400 Bad Request` (validation/business)
- `401 Unauthorized` / `403 Forbidden`
- `404 Not Found`
- `409 Conflict` (e.g., duplicate, velocity/limit)
- `422 Unprocessable Entity` (semantic validation)
- `429 Too Many Requests`
- `500 Internal Server Error` / `502 Bad Gateway` / `503 Service Unavailable`

## Conventions
- Time: ISO8601 with timezone (e.g., `2025-08-27T08:20:17+07:00`)
- Money: string decimal with 2–4 fraction digits; include `currency` (ISO 4217)
- IDs: strings (ULID/UUID) opaque to clients
- Pagination: `page`, `pageSize`, `nextPageToken` (if streaming)
