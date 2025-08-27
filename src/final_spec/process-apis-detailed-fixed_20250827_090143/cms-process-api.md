# cms-process-api @ 1.0.0

## Overview
Get home content via CMS and return as-is (or minimally reshaped).

## Global Headers (per GlobalContract)
- `xapp-trace-id`: **string(UUID)** — required
- `Authorization`: **Bearer &lt;JWT&gt;** — required unless explicitly public
- `Accept-Language`: **th-TH | en-US** — optional

**Response Header:** `xapp-source: cms-process-api@1.0.0`  
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
## Home
**GET /v1/process/cms/home**  
Return CMS home content.



### Response Fields
| Field | Type | Description |
|---|---|---|
| banners | array<object> | Banner list |
| sections | array<object> | Section blocks |
| traceid | string | Trace id |

#### Response Example
```json
{ "banners":[{"id":"bnr1","title":"Promo"}],"sections":[],"traceid":"1f3c..." }
```
