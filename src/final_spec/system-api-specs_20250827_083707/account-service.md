# account-service @ 1.0.0

> **Base Path**: `/xapi/v1/accounts`  
> **xapp-source**: `account-service@1.0.0`

## GET /xapi/v1/accounts
รายการบัญชีของผู้ใช้

### Query
- `page`, `pageSize` (optional)

### Response 200
```json
{
  "items": [
    { "accountId":"...", "type":"SAVING", "numberMasked":"xxx-xxx-1234", "currency":"THB", "status":"ACTIVE" }
  ],
  "nextPageToken": null
}
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| ACC-LIST-SYS | 500 | ระบบขัดข้อง |

---

## GET /xapi/v1/accounts/{accountId}/balance
ยอดคงเหลือบัญชี

### Response 200
```json
{ "accountId":"...", "available":"1000.00", "ledger":"1000.00", "currency":"THB", "asOf":"ISO8601" }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| ACC-BAL-404 | 404 | ไม่พบบัญชี |
| ACC-BAL-SYS | 500 | ระบบขัดข้อง |

---

## GET /xapi/v1/accounts/{accountId}/transactions
เดินบัญชี

### Query
- `from`, `to` (ISO8601), `page`, `pageSize`

### Response 200
```json
{
  "items":[
    { "txnId":"...", "postedAt":"ISO8601", "amount":"-100.00", "currency":"THB", "type":"DEBIT", "desc":"Transfer to ..." }
  ],
  "nextPageToken": null
}
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| ACC-TXN-404 | 404 | ไม่พบบัญชี |
| ACC-TXN-SYS | 500 | ระบบขัดข้อง |
