# dashboard-service @ 1.0.0

> **Base Path**: `/xapi/v1/dashboard`  
> **xapp-source**: `dashboard-service@1.0.0`

## GET /xapi/v1/dashboard
โหลดวิดเจ็ตหน้าแรก

### Response 200
```json
{
  "cards":[
    { "type":"balance_summary", "accounts":[{"accountId":"...","available":"1000.00","currency":"THB"}] },
    { "type":"recent_txn", "items":[{"txnId":"...","amount":"-50.00","desc":"..." }] }
  ]
}
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| DASH-SYS | 500 | ระบบขัดข้อง |
