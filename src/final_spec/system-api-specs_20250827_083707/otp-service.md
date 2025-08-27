# otp-service @ 1.0.0

> **Base Path**: `/sapi/v1/otp`  
> **xapp-source**: `otp-service@1.0.0`

## POST /sapi/v1/otp/send
ขอส่ง OTP

### Request
| Field | Type | Req | Notes |
|---|---|---|---|
| to | string | Y | mobile/email |
| channel | enum | Y | sms \| email \| push-code |
| purpose | string | Y | register \| login \| transfer \| withdraw |
| ttlSec | number | N | default 300 |
| template | string | N | ex: OTP_GENERIC |

### Response 200
```json
{ "otpRefId":"...", "expireAt":"ISO8601" }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| OTP-SEND-RATE | 429 | ส่งถี่เกินไป |
| OTP-SEND-DEST | 400 | ที่อยู่ปลายทางไม่ถูกต้อง |
| OTP-SEND-SYS | 500 | ระบบขัดข้อง |

---

## POST /sapi/v1/otp/verify
ยืนยัน OTP

### Request
| Field | Type | Req |
|---|---|---|
| otpRefId | string | Y |
| code | string | Y |

### Response 200
```json
{ "verified": true }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| OTP-VERIFY-INVALID | 400 | OTP ไม่ถูกต้อง |
| OTP-VERIFY-EXPIRED | 400 | OTP หมดอายุ |
| OTP-VERIFY-SYS | 500 | ระบบขัดข้อง |
