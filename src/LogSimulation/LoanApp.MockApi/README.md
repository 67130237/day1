# LoanApp.MockApi (Upgraded for Realistic Logging & Fault Injection)

Run:
```
dotnet restore
dotnet run
```
Swagger: http://localhost:5080/swagger

## Admin / Faults
- POST `/_admin/faults/load`  (send JSON config)
- POST `/_admin/faults/toggle/{enabled}`
- GET  `/_admin/faults`

## Example fault config
```json
{
  "enabled": true,
  "profiles": {
    "default": { "latencyMs": 0, "errorRate": 0.0, "abortHttpStatus": 0 },
    "flaky-network": { "latencyMs": 200, "errorRate": 0.15, "abortHttpStatus": 0 },
    "heavy-db-lag": { "latencyMs": 1500, "errorRate": 0.05, "abortHttpStatus": 0 },
    "psp-outage": { "latencyMs": 0, "errorRate": 0.0, "abortHttpStatus": 503 }
  },
  "routes": [
    { "path": "/api/v1/payments", "methods": ["POST","GET"], "profile": "flaky-network" },
    { "path": "/api/v1/loan-applications", "methods": ["POST"], "profile": "heavy-db-lag" }
  ],
  "schedule": [
    { "fromSec": 300, "toSec": 600, "profile": "psp-outage" }
  ]
}
```
Set request header `X-Fault-Profile: psp-outage` to override per request.