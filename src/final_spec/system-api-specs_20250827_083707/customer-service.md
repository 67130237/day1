# customer-service @ 1.0.0

> **Base Path**: `/xapi/v1/customers`  
> **xapp-source**: `customer-service@1.0.0`

## GET /xapi/v1/customers/me
ข้อมูลลูกค้า (ย่อ)

### Response 200
```json
{
  "userId":"...",
  "fullName":"...",
  "mobile":"+66...",
  "email":"...",
  "kycLevel":"basic",
  "preferences":{ "lang":"th-TH", "marketing": false }
}
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| CUST-NOTFOUND | 404 | ไม่พบข้อมูลลูกค้า |
| CUST-SYS | 500 | ระบบขัดข้อง |

---

## PUT /xapi/v1/customers/me/settings
อัปเดตการตั้งค่า

### Request
| Field | Type | Req |
|---|---|---|
| preferences | object | Y | { "lang": "...", "marketing": true/false } |

### Response 200
```json
{ "updated": true }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| CUST-SET-VAL | 400 | รูปแบบข้อมูลไม่ถูกต้อง |
| CUST-SET-SYS | 500 | ระบบขัดข้อง |
