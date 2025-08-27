# disbursement-service @ 1.0.0

> **Base Path**: `/papi/v1/disbursements`  
> **xapp-source**: `disbursement-service@1.0.0`

## POST /papi/v1/disbursements/eligibility
ตรวจคุณสมบัติการปล่อยจ่าย

### Request
| Field | Type | Req |
|---|---|---|
| userId | string | Y |
| productCode | string | Y |
| amount | string | Y |
| currency | string | Y |

### Response 200
```json
{ "eligible": true, "maxAmount":"50000.00","currency":"THB","reason":"" }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| DISB-ELIG-VAL | 400 | ข้อมูลไม่ถูกต้อง |
| DISB-ELIG-NOT | 200 | eligible=false |
| DISB-ELIG-SYS | 500 | ระบบขัดข้อง |

---

## POST /papi/v1/disbursements
เริ่มกระบวนการปล่อยจ่าย

### Request
| Field | Type | Req |
|---|---|---|
| contractId | string | Y |
| payToAccountId | string | Y |
| amount | string | Y |
| currency | string | Y |
| idempotencyKey | string | Y | header |

### Response 202
```json
{ "disbursementId":"...", "status":"PROCESSING" }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| DISB-START-VAL | 400 | ข้อมูลไม่ถูกต้อง |
| DISB-START-DUP | 409 | ทำซ้ำ (Idempotent) |
| DISB-START-SYS | 500 | ระบบขัดข้อง |
