# transfer-process-api @ 1.0.0

## Overview
Prepare transfer → request OTP → confirm transfer with commit and push.

## Global Headers (per GlobalContract)
- `xapp-trace-id`: **string(UUID)** — required
- `Authorization`: **Bearer &lt;JWT&gt;** — required unless explicitly public
- `Idempotency-Key`: **string** — for idempotent *write* endpoints
- `Accept-Language`: **th-TH | en-US** — optional

**Response Header:** `xapp-source: transfer-process-api@1.0.0`  
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
**POST /v1/process/transfer/prepare**  
Check balance, get quote/limit/AML flags.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| sourceAccountId | string | yes | Source account |
| dest.accountNo | string | yes | Destination account |
| dest.bankCode | string | yes | Destination bank code |
| dest.name | string | yes | Beneficiary name |
| amount | string(decimal) | yes | Amount |
| currency | string | yes | ISO 4217 |

#### Request Example
```json
{
  "sourceAccountId":"acc_01H",
  "dest":{"accountNo":"1234567890","bankCode":"004","name":"Arisa"},
  "amount":"500.00",
  "currency":"THB"
}
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| fee | string | Fee amount |
| currency | string | Currency |
| limitOk | boolean | Limit check |
| amlFlag | boolean | AML indicator |
| message | string | Advisory |
| traceid | string | Trace id |

#### Response Example
```json
{ "fee":"10.00","currency":"THB","limitOk":true,"amlFlag":false,"message":"","traceid":"1f3c..." }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PTRF-VAL-001 | 400 | Invalid transfer data
PTRF-LIMIT-001 | 409 | Exceeds limit
PTRF-SYS-001 | 500 | Internal error

---
## Request OTP
**POST /v1/process/transfer/request-otp**  
Send OTP for transfer confirmation.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| mobile | string(E.164) | yes | Destination for OTP (or resolve by user) |
| purpose | string | yes | 'transfer' |

#### Request Example
```json
{ "mobile":"+66999999999", "purpose":"transfer" }
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| otpRefId | string | OTP reference |
| expireAt | string(ISO8601) | Expiry |

#### Response Example
```json
{ "otpRefId":"otp_01H...", "expireAt":"2025-08-27T09:00:00+07:00" }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PTRF-OTP-RATE | 429 | Too many requests
PTRF-OTP-SYS | 500 | Internal error

---
## Confirm
**POST /v1/process/transfer/confirm**  
Verify OTP and commit transfer; push success/failed.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| otpRefId | string | yes | Reference |
| code | string | yes | OTP code |
| sourceAccountId | string | yes | Source |
| dest | object | yes | Same as prepare |
| amount | string | yes | Amount |
| currency | string | yes | ISO 4217 |
| note | string | no | Narrative |

#### Request Example
```json
{
  "otpRefId":"otp_01H...",
  "code":"123456",
  "sourceAccountId":"acc_01H",
  "dest":{"accountNo":"1234567890","bankCode":"004","name":"Arisa"},
  "amount":"500.00","currency":"THB","note":"gift"
}
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| transferId | string | Transfer id |
| status | enum(SUCCESS,FAILED,QUEUED) | Final state |
| expectedSettleAt | string(ISO8601)? | If queued |
| traceid | string | Trace id |

#### Response Example
```json
{ "transferId":"trf_01H...", "status":"SUCCESS", "expectedSettleAt":null, "traceid":"1f3c..." }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PTRF-AUTH-001 | 401 | OTP invalid/expired
PTRF-VAL-002 | 409 | Insufficient balance / velocity
PTRF-EXT-001 | 502 | Upstream transfer failed
PTRF-SYS-002 | 500 | Internal error
