# dashboard-process-api @ 1.0.0

## Overview
Proxy/compose dashboard widgets from dashboard-service.

## Global Headers (per GlobalContract)
- `xapp-trace-id`: **string(UUID)** — required
- `Authorization`: **Bearer &lt;JWT&gt;** — required unless explicitly public
- `Accept-Language`: **th-TH | en-US** — optional

**Response Header:** `xapp-source: dashboard-process-api@1.0.0`  
**Error Envelope (all endpoints):**
```json
{
  "code": "STRING",
  "message": "STRING",
  "traceId": "UUID",
  "details": {}
}
```

---
## Load Dashboard
**GET /v1/process/dashboard/load**  
Return composed dashboard widgets for main page.



### Response Fields
| Field | Type | Description |
|---|---|---|
| cards | array<object> | Dashboard cards |
| traceid | string | Trace id |

#### Response Example
```json
{ "cards":[{"type":"balance_summary","accounts":[{"accountId":"...","available":"1000.00","currency":"THB"}]}], "traceid":"1f3c..." }
```
