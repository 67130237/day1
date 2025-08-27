# bootstrap-process-api @ 1.0.0

## Overview
Compose app public settings and CMS home content into a single bootstrap payload.

## Global Headers (per GlobalContract)
- `xapp-trace-id`: **string(UUID)** — required
- `Authorization`: **Bearer &lt;JWT&gt;** — required unless explicitly public
- `Idempotency-Key`: **string** — for idempotent *write* endpoints
- `Accept-Language`: **th-TH | en-US** — optional

**Response Header:** `xapp-source: bootstrap-process-api@1.0.0`  
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
## Bootstrap Init
**GET /v1/process/bootstrap/init**  
Fetch public appsettings and CMS home.



### Response Fields
| Field | Type | Description |
|---|---|---|
| appsettings | object | App settings (public) |
| home | object | CMS home content |
| traceid | string | Trace id |

#### Response Example
```json
{
  "appsettings": { "minVersion":"1.2.3", "featureFlags":{"transfer_otp":true} },
  "home": { "banners":[{"id":"bnr1","title":"Promo"}], "sections":[] },
  "traceid": "1f3c..."
}
```
