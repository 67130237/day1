# settings-process-api @ 1.0.0

## Overview
Bootstrap public settings and update user settings.

## Global Headers (per GlobalContract)
- `xapp-trace-id`: **string(UUID)** — required
- `Authorization`: **Bearer &lt;JWT&gt;** — required unless explicitly public
- `Idempotency-Key`: **string** — for idempotent *write* endpoints
- `Accept-Language`: **th-TH | en-US** — optional

**Response Header:** `xapp-source: settings-process-api@1.0.0`  
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
## Bootstrap
**GET /v1/process/settings/bootstrap**  
Fetch app public settings for Settings page.



### Response Fields
| Field | Type | Description |
|---|---|---|
| appsettings | object | Public settings |
| traceid | string | Trace id |

#### Response Example
```json
{ "appsettings": { "minVersion":"1.2.3" }, "traceid":"1f3c..." }
```



---
## Update
**PUT /v1/process/settings/update**  
Update user preferences via customer-service.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| preferences.lang | string | no | th-TH | en-US |
| preferences.marketing | boolean | no | Opt-in marketing |

#### Request Example
```json
{ "preferences": { "lang":"th-TH", "marketing": false } }
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| updated | boolean | Update result |
| traceid | string | Trace id |

#### Response Example
```json
{ "updated": true, "traceid": "1f3c..." }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PSET-VAL-001 | 400 | Invalid preferences
PSET-SYS-001 | 500 | Internal error
