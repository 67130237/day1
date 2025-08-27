# dopa-service @ 1.0.0

> **Base Path**: `/sapi/v1/dopa`  
> **xapp-source**: `dopa-service@1.0.0`

## POST /sapi/v1/dopa/verify
ตรวจสอบกับ DOPA

### Request
| Field | Type | Req |
|---|---|---|
| citizenId | string(13) | Y |
| firstName | string | Y |
| lastName | string | Y |
| birthDate | string | Y |
| laserCode | string | N |

### Response 200
```json
{ "verified": true, "score": 0.98, "provider":"DOPA", "checkedAt":"ISO8601" }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| DOPA-VAL | 400 | ข้อมูลไม่ครบ/ไม่ถูกต้อง |
| DOPA-NOTMATCH | 200 | verified=false |
| DOPA-UPSTREAM | 502 | ช่องทางเชื่อมต่อขัดข้อง |
| DOPA-SYS | 500 | ระบบขัดข้อง |
