# login-process-api @ 1.0.0

## Overview
PIN login with optional OTP and biometric login; may emit sign-in push.

## Global Headers (per GlobalContract)
- `xapp-trace-id`: **string(UUID)** — required
- `Authorization`: **Bearer &lt;JWT&gt;** — required unless explicitly public
- `Accept-Language`: **th-TH | en-US** — optional

**Response Header:** `xapp-source: login-process-api@1.0.0`  
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
## PIN Init
**POST /v1/process/login/pin/init**  
Validate PIN and decide whether MFA is required.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| mobile | string(E.164) | yes | Login identifier |
| pin | string(6) | yes | Numeric PIN |
| deviceId | string | yes | Device identifier |

#### Request Example
```json
{ "mobile":"+66999999999","pin":"123456","deviceId":"A1B2" }
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| mfaRequired | boolean | OTP is required or not |
| reason | string | Optional reason (e.g., newDevice) |

#### Response Example
```json
{ "mfaRequired": true, "reason":"newDevice" }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PLOG-AUTH-001 | 401 | Invalid PIN
PLOG-AUTH-002 | 403 | Account locked
PLOG-SYS-001 | 500 | Internal error

---
## Request OTP
**POST /v1/process/login/pin/request-otp**  
Send OTP for MFA.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| mobile | string(E.164) | yes | Destination MSISDN |

#### Request Example
```json
{ "mobile":"+66999999999" }
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| otpRefId | string | OTP reference |
| expireAt | string(ISO8601) | Expiry time |

#### Response Example
```json
{ "otpRefId":"otp_01H","expireAt":"2025-08-27T09:00:00+07:00" }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PLOG-OTP-RATE | 429 | Too many requests
PLOG-OTP-SYS | 500 | Internal error

---
## Confirm
**POST /v1/process/login/pin/confirm**  
Verify OTP and return tokens; may emit SIGNIN push.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| otpRefId | string | yes | Reference id |
| code | string | yes | 6-digit code |

#### Request Example
```json
{ "otpRefId":"otp_01H","code":"123456" }
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| accessToken | string | Access token |
| refreshToken | string | Refresh token |
| expiresIn | number | Seconds to expiry |

#### Response Example
```json
{ "accessToken":"...","refreshToken":"...","expiresIn":3600 }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PLOG-OTP-INVALID | 401 | OTP invalid/expired
PLOG-SYS-002 | 500 | Internal error

---
## Biometric Verify
**POST /v1/process/login/biometric/verify**  
Verify biometric assertion and return tokens.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| deviceId | string | yes | Device id |
| assertion | string | yes | Platform assertion |
| nonce | string | yes | Anti-replay nonce |

#### Request Example
```json
{ "deviceId":"A1B2","assertion":"BASE64...","nonce":"xyz" }
```


### Response Fields
| Field | Type | Description |
|---|---|---|
| accessToken | string | Access token |
| refreshToken | string | Refresh token |
| expiresIn | number | Lifetime seconds |

#### Response Example
```json
{ "accessToken":"...","refreshToken":"...","expiresIn":3600 }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PLOG-BIO-INVALID | 401 | Invalid assertion
PLOG-BIO-NEWDEVICE | 403 | Must verify new device
PLOG-SYS-003 | 500 | Internal error
