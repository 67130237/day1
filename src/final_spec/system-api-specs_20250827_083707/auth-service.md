# auth-service @ 1.0.0

Implements registration and authentication flows.

> **Base Path**: `/xapi/v1/auth`  
> **xapp-source**: `auth-service@1.0.0`

## POST /xapi/v1/auth/register
สมัครผู้ใช้ใหม่

### Request (JSON)
| Field | Type | Req | Notes |
|---|---|---|---|
| mobile | string (E.164) | Y | ใช้เป็น primary login |
| email | string | N |  |
| pin | string (6) | Y | 0–9 only |
| firstName | string | Y |  |
| lastName | string | Y |  |
| citizenId | string (13) | Y |  |
| acceptTerms | boolean | Y | ต้องเป็น true |
| metadata | object | N | deviceId, os, appVersion |

### Response 201
```json
{ "userId":"...", "registeredAt":"ISO8601" }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| AUTH-REG-VAL | 400 | ข้อมูลสมัครไม่ถูกต้อง |
| AUTH-REG-DUP | 409 | ผู้ใช้นี้มีอยู่แล้ว |
| AUTH-REG-SYS | 500 | ระบบขัดข้อง |

---

## POST /xapi/v1/auth/login/pin
ล็อกอินด้วย PIN

### Request
| Field | Type | Req |
|---|---|---|
| mobile | string | Y |
| pin | string (6) | Y |
| deviceId | string | Y |

### Response 200
```json
{ "accessToken":"...", "refreshToken":"...", "expiresIn":3600, "mfaRequired":false }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| AUTH-PIN-INVALID | 401 | PIN ไม่ถูกต้อง |
| AUTH-PIN-LOCKED | 403 | บัญชีถูกล็อค |
| AUTH-PIN-SYS | 500 | ระบบขัดข้อง |

---

## POST /xapi/v1/auth/login/biometric
ล็อกอินด้วยไบโอเมตริก

### Request
| Field | Type | Req |
|---|---|---|
| deviceId | string | Y |
| assertion | string | Y |
| nonce | string | Y |

### Response 200
```json
{ "accessToken":"...", "refreshToken":"...", "expiresIn":3600 }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| AUTH-BIO-INVALID | 401 | Assertion ไม่ถูกต้อง |
| AUTH-BIO-NEWDEVICE | 403 | ต้องยืนยันอุปกรณ์ใหม่ |
| AUTH-BIO-SYS | 500 | ระบบขัดข้อง |
