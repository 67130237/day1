# appsettings-service @ 1.0.0

> **Base Path**: `/xapi/v1/appsettings`  
> **xapp-source**: `appsettings-service@1.0.0`

## GET /xapi/v1/appsettings
ดึงค่าคอนฟิกสาธารณะ

### Query
- `scope=public` (required)

### Response 200
```json
{ "minVersion":"1.2.3", "featureFlags": { "transfer_otp": true } }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| APPS-VAL | 400 | scope ไม่รองรับ |
| APPS-SYS | 500 | ระบบขัดข้อง |
