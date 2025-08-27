# kyc-process-api @ 1.0.0

## Overview
Verify identity via DOPA (through identity-service).

## Global Headers (per GlobalContract)
- `xapp-trace-id`: **string(UUID)** — required
- `Authorization`: **Bearer &lt;JWT&gt;** — required unless explicitly public
- `Idempotency-Key`: **string** — for idempotent *write* endpoints
- `Accept-Language`: **th-TH | en-US** — optional

**Response Header:** `xapp-source: kyc-process-api@1.0.0`  
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
## Verify
**POST /v1/process/kyc/verify**  
Verify with DOPA via identity-service; return verification result.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| citizenId | string(13) | yes | Thai ID |
| firstName | string | yes | Given name |
| lastName | string | yes | Surname |
| birthDate | string(YYYY-MM-DD) | yes | DOB |
| laserCode | string | no | Optional |

#### Request Example
```json
{ "citizenId":"1234567890123","firstName":"Somchai","lastName":"Jaidee","birthDate":"1990-05-10" }
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| verified | boolean | DOPA result |
| matchedFields | array[string] | Matched fields |
| traceid | string | Trace id |

#### Response Example
```json
{ "verified": true, "matchedFields":["firstName","lastName","birthDate"], "traceid":"1f3c..." }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PKYC-VAL-001 | 400 | Invalid data
PKYC-UPSTREAM | 502 | DOPA unavailable
PKYC-SYS-001 | 500 | Internal error
