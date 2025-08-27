# withdraw-process-api @ 1.0.0

## Overview
Validate locally, create withdrawal record, push notification.

## Global Headers (per GlobalContract)
- `xapp-trace-id`: **string(UUID)** — required
- `Authorization`: **Bearer &lt;JWT&gt;** — required unless explicitly public
- `Idempotency-Key`: **string** — for idempotent *write* endpoints
- `Accept-Language`: **th-TH | en-US** — optional

**Response Header:** `xapp-source: withdraw-process-api@1.0.0`  
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
## Prepare
**POST /v1/process/withdraw/prepare**  
Local validations only before confirm.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| sourceAccountId | string | yes | Source account |
| channel | enum(atm,counter) | yes | Withdrawal channel |
| amount | string | yes | Amount |
| currency | string | yes | ISO 4217 |

#### Request Example
```json
{ "sourceAccountId":"acc_01H","channel":"atm","amount":"1000.00","currency":"THB" }
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| confirmable | boolean | Whether can proceed |
| message | string | Advisory message |
| traceid | string | Trace id |

#### Response Example
```json
{ "confirmable": true, "message":"", "traceid":"1f3c..." }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PWDR-VAL-001 | 409 | Insufficient balance/limit
PWDR-SYS-001 | 500 | Internal error

---
## Confirm
**POST /v1/process/withdraw/confirm**  
Create withdrawal order and push notification.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| sourceAccountId | string | yes | Source account |
| channel | enum(atm,counter) | yes | Channel |
| amount | string | yes | Amount |
| currency | string | yes | ISO 4217 |

#### Request Example
```json
{ "sourceAccountId":"acc_01H","channel":"atm","amount":"1000.00","currency":"THB" }
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| withdrawalId | string | Withdrawal id |
| status | enum(issued,redeemed,expired,canceled) | Current state |
| token | string? | Token for ATM/counter |
| expireAt | string(ISO8601)? | Expiration |
| traceid | string | Trace id |

#### Response Example
```json
{ "withdrawalId":"wd_01H...","status":"issued","token":"ABC123","expireAt":"2025-08-28T09:00:00+07:00","traceid":"1f3c..." }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PWDR-VAL-002 | 409 | Insufficient balance/limit
PWDR-EXT-001 | 502 | Channel provider unavailable
PWDR-SYS-002 | 500 | Internal error
