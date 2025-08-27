# identity-service @ 1.0.0

> **Base Path**: `/papi/v1/identity`  
> **xapp-source**: `identity-service@1.0.0`

## POST /papi/v1/identity
สร้าง/เปิดใช้งาน identity ภายใน

### Request
| Field | Type | Req |
|---|---|---|
| userId | string | Y |
| kycLevel | enum | N | none \| basic \| full |

### Response 201
```json
{ "identityId":"...", "userId":"...", "kycLevel":"basic", "activatedAt":"ISO8601" }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| IDEN-UPD-404 | 404 | ไม่พบผู้ใช้ |
| IDEN-UPD-SYS | 500 | ระบบขัดข้อง |

---

## POST /papi/v1/identity/verify-dopa
ส่งคำขอตรวจ DOPA (proxy ไป dopa-service)

### Request
| Field | Type | Req |
|---|---|---|
| citizenId | string(13) | Y |
| firstName | string | Y |
| lastName | string | Y |
| birthDate | string(YYYY-MM-DD) | Y |
| laserCode | string | N |

### Response 200
```json
{ "verified": true, "matchedFields": ["firstName","lastName","birthDate"] }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| IDEN-DOPA-VAL | 400 | รูปแบบข้อมูลไม่ถูกต้อง |
| IDEN-DOPA-UPSTREAM | 502 | DOPA ไม่พร้อมใช้งาน |
| IDEN-DOPA-SYS | 500 | ระบบขัดข้อง |
