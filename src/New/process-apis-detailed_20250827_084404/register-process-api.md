# register-process-api @ 1.0.0

## Overview
Orchestrates: register → send OTP → verify OTP → verify DOPA → activate + Welcome Push

## Global Headers (per GlobalContract)
- `xapp-trace-id`: **string(UUID)** — required
- `Authorization`: **Bearer &lt;JWT&gt;** — required unless explicitly public
- `Idempotency-Key`: **string** — for idempotent *write* endpoints
- `Accept-Language`: **th-TH | en-US** — optional

**Response Header:** `xapp-source: register-process-api@1.0.0`  
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
## Init Registration
**POST /v1/process/register/init**  
Validate input, create provisional user, and send OTP.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| mobile | string(E.164) | yes | Primary login number |
| email | string | no | Optional email |
| citizenId | string(13) | yes | Thai ID |
| firstName | string | yes | Given name |
| lastName | string | yes | Surname |
| pin | string(6) | yes | Numeric PIN |
| acceptTerms | boolean | yes | Must be true |
| channel | enum(mobile,web) | yes | Source channel |
| metadata | object | no | deviceId, os, appVersion |

#### Request Example
```json
{
  "mobile": "+66999999999",
  "email": "user@example.com",
  "citizenId": "1234567890123",
  "firstName": "Somchai",
  "lastName": "Jaidee",
  "pin": "123456",
  "acceptTerms": true,
  "channel": "mobile",
  "metadata": { "deviceId": "A1B2", "os": "iOS", "appVersion": "2.3.1" }
}
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| userId | string | Provisional user id |
| nextStep | string | 'otp' |
| otpRefId | string | OTP reference id |
| expireAt | string(ISO8601) | OTP expiry |

#### Response Example
```json
{ "userId":"usr_01H...", "nextStep":"otp", "otpRefId":"otp_01H...", "expireAt":"2025-08-27T09:00:00+07:00" }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PREG-VAL-001 | 400 | Invalid or incomplete registration data
PREG-VAL-002 | 409 | User already exists
PREG-EXT-001 | 502 | OTP provider unavailable
PREG-SYS-001 | 500 | Internal error

---
## Verify OTP
**POST /v1/process/register/verify-otp**  
Verify OTP for the provisional user.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| otpRefId | string | yes | OTP reference id |
| code | string | yes | 6-digit code |

#### Request Example
```json
{ "otpRefId":"otp_01H...", "code":"123456" }
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| verified | boolean | Whether OTP is valid |
| nextStep | string | 'dopa-verify' if verified |

#### Response Example
```json
{ "verified": true, "nextStep":"dopa-verify" }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PREG-VAL-003 | 400 | OTP invalid or expired
PREG-SYS-002 | 500 | Internal error

---
## Verify DOPA
**POST /v1/process/register/verify-dopa**  
Call identity-service to verify with DOPA (may call dopa-service upstream).

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| citizenId | string(13) | yes | Thai ID |
| firstName | string | yes | Given name |
| lastName | string | yes | Surname |
| birthDate | string(YYYY-MM-DD) | yes | DOB |
| laserCode | string | no | Optional laser code |

#### Request Example
```json
{ "citizenId":"1234567890123","firstName":"Somchai","lastName":"Jaidee","birthDate":"1990-05-10","laserCode":"JT0-000..." }
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| verified | boolean | DOPA verification result |
| matchedFields | array[string] | Fields matched by DOPA |

#### Response Example
```json
{ "verified": true, "matchedFields": ["firstName","lastName","birthDate"] }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PREG-VAL-004 | 400 | Invalid KYC fields
PREG-EXT-002 | 502 | DOPA upstream unavailable
PREG-SYS-003 | 500 | Internal error

---
## Activate
**POST /v1/process/register/activate**  
Create identity, fetch minimal profile & accounts, emit Welcome push.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| userId | string | yes | Provisional user id |

#### Request Example
```json
{ "userId":"usr_01H..." }
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| userId | string | Final user id |
| kycStatus | enum(none,basic,full) | KYC state |
| accountsCount | number | Number of accounts |
| message | string | Human-readable confirmation |

#### Response Example
```json
{ "userId":"usr_01H...", "kycStatus":"basic", "accountsCount":1, "message":"Activated" }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PREG-DB-001 | 404 | User not found
PREG-EXT-003 | 502 | Downstream dependency failed
PREG-SYS-004 | 500 | Internal error
