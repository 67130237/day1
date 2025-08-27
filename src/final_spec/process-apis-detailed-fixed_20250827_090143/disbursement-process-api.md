# disbursement-process-api @ 1.0.0

## Overview
Check eligibility then start disbursement; push notification.

## Global Headers (per GlobalContract)
- `xapp-trace-id`: **string(UUID)** — required
- `Authorization`: **Bearer &lt;JWT&gt;** — required unless explicitly public
- `Accept-Language`: **th-TH | en-US** — optional

**Response Header:** `xapp-source: disbursement-process-api@1.0.0`  
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
## Eligibility
**POST /v1/process/disbursement/eligibility**  
Check product eligibility and limits.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| userId | string | yes | Target user |
| productCode | string | yes | Product identifier |
| amount | string | yes | Requested amount |
| currency | string | yes | ISO 4217 |

#### Request Example
```json
{ "userId":"usr_01H","productCode":"LN01","amount":"20000.00","currency":"THB"}
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| eligible | boolean | Eligibility flag |
| maxAmount | string | Max amount allowed |
| currency | string | Currency |
| reason | string | Reason if not eligible |
| traceid | string | Trace id |

#### Response Example
```json
{ "eligible": true, "maxAmount":"50000.00","currency":"THB","reason":"","traceid":"1f3c..." }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PDISB-ELIG-VAL | 400 | Invalid data
PDISB-ELIG-SYS | 500 | Internal error

---
## Start
**POST /v1/process/disbursement/start**  
Start disbursement and push notification.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| contractId | string | yes | Contract id |
| payToAccountId | string | yes | Credit account id |
| amount | string | yes | Amount |
| currency | string | yes | ISO 4217 |

#### Request Example
```json
{ "contractId":"ctr_01H","payToAccountId":"acc_01H","amount":"20000.00","currency":"THB"}
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| disbursementId | string | Disbursement id |
| status | enum(PROCESSING,SUCCESS,FAILED) | Current status |
| traceid | string | Trace id |

#### Response Example
```json
{ "disbursementId":"dsb_01H","status":"PROCESSING","traceid":"1f3c..." }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PDISB-START-DUP | 409 | Duplicate request
PDISB-START-SYS | 500 | Internal error
