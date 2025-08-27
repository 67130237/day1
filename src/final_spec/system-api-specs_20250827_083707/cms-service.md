# cms-service @ 1.0.0

> **Base Path**: `/xapi/v1/cms`  
> **xapp-source**: `cms-service@1.0.0`

## GET /xapi/v1/cms/home
โหลดคอนเทนต์หน้า Home

### Query
- `segment` (optional)

### Response 200
```json
{ "banners":[{ "id":"...", "title":"...", "imageUrl":"...", "actionUrl":"..." }], "sections":[...] }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| CMS-HOME-SYS | 500 | ระบบขัดข้อง |

---

## GET /xapi/v1/cms/banners
แบนเนอร์ตามตำแหน่ง

### Query
- `position` (required) e.g., `home_top`

### Response 200
```json
{ "items":[{ "id":"...", "position":"home_top", "title":"...", "imageUrl":"...", "actionUrl":"..." } ]}
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| CMS-BNR-VAL | 400 | position ไม่ถูกต้อง |
| CMS-BNR-SYS | 500 | ระบบขัดข้อง |
