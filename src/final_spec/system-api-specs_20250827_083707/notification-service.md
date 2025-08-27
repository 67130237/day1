# notification-service @ 1.0.0

> **Base Path**: `/papi/v1/notifications`  
> **xapp-source**: `notification-service@1.0.0`

## POST /papi/v1/notifications
ร้องขอส่ง Push Notification (no inbox/realtime)

### Request
| Field | Type | Req | Notes |
|---|---|---|---|
| to | object | Y | { "userId":"..." } หรือ { "topic":"..." } |
| template | string | Y | e.g., TRANSFER_SUCCESS |
| data | object | N | key-value for template |
| dedupKey | string | N | ป้องกันซ้ำ |
| scheduleAt | string(ISO8601) | N | ส่งล่วงหน้า |

### Response 202
```json
{ "requestId":"...", "accepted": true }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| NOTI-VAL | 400 | รูปแบบไม่ถูกต้อง |
| NOTI-QUOTA | 429 | เกินโควต้า |
| NOTI-SYS | 500 | ระบบขัดข้อง |
