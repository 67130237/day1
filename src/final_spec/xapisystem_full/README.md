# System Services (Minimal APIs) — Fault Injection + Elastic APM

ชุด **system services** ตามสเปกในโปรเจกต์ (controller only, ไม่มีการเรียกต่อ) พร้อม Fault Injection ที่ชั้น controller และบันทึก log/trace ไป Elastic APM

## Services & Endpoints
- **dopa-service** (`http://localhost:7101`)  
  - `POST /sapi/v1/dopa/verify` — body: `{ citizenId, firstName, lastName, birthDate, laserCode? }`
- **otp-service** (`http://localhost:7102`)  
  - `POST /sapi/v1/otp/send` — body: `{ to, channel, purpose, ttlSec?, template? }`
  - `POST /sapi/v1/otp/verify` — body: `{ otpRefId, code }`
- **notification-service** (`http://localhost:7103`)  
  - `POST /papi/v1/notifications` — body: `{ to:{userId|topic}, template, data?, dedupKey?, scheduleAt? }`
- **appsettings-service** (`http://localhost:7104`)  
  - `GET /xapi/v1/appsettings?scope=public`
- **cms-service** (`http://localhost:7105`)  
  - `GET /xapi/v1/cms/home?segment=`
  - `GET /xapi/v1/cms/banners?position=`

> ทุก response แนบ `xapp-source: <service>@<version>` และ error envelope `{ code, message, traceId, details }`

## Fault Injection Header
ใช้ header `X-Fault-Inject` (**base64 ของ JSON**) — ทำงานเฉพาะชั้น `controller`:
```json
{
  "type": "delay|http_error|exception|business_error|cancel|timeout",
  "atService": "dopa|otp|notification|appsettings|cms",
  "atCodeLayer": "controller",
  "params": { "delayMs": 1500, "httpStatus": 503, "message": "simulated", "timeoutMs": 5000 }
}
```

### ตัวอย่าง: 503 ที่ dopa
```bash
HDR=$(printf '%s' '{"type":"http_error","atService":"dopa","atCodeLayer":"controller","params":{"httpStatus":503,"message":"oops"}}' | base64)
curl -sS -X POST http://localhost:7101/sapi/v1/dopa/verify   -H "Content-Type: application/json"   -H "X-Fault-Inject: $HDR"   -H "xapp-trace-id: 11111111-1111-1111-1111-111111111111"   -d '{"citizenId":"1234567890123","firstName":"A","lastName":"B","birthDate":"1990-01-01"}' | jq
```

## Run (Docker)
```bash
docker compose up --build
```
APM server จะเปิดที่ `http://localhost:8200`. ตั้งค่า env ใน compose แล้ว agent (`Elastic.Apm.AspNetCore`) จะส่ง trace ให้โดยอัตโนมัติ

## โครงสร้างโค้ด
```
.
├─ docker-compose.yml
├─ Directory.Build.props
├─ src/
│  ├─ shared/Project.Shared                 # ErrorEnvelope, FaultMiddleware, etc.
│  └─ system/
│     ├─ dopa/DopaService
│     ├─ otp/OtpService
│     ├─ notification/NotificationService
│     ├─ appsettings/AppSettingsService
│     └─ cms/CmsService
```

## หมายเหตุ
- โค้ดนี้ให้ response “สมจริงตาม request” พร้อม validation ขั้นต้นตามสเปก
- ไม่มีการ forward header หรือ outbound calls ใดๆ

## เพิ่มเติม (update)
- บังคับ header `xapp-trace-id` ทุก endpoint (GlobalContract) — หากไม่มีจะตอบ 400 `<PFX>-VAL` พร้อม message `xapp-trace-id required`
- เพิ่มบริการตามเอกสาร: **account, transaction, identity, dashboard, customer, disbursement, auth**

### พอร์ตเพิ่ม
- account:       http://localhost:7106
- transaction:   http://localhost:7107
- identity:      http://localhost:7108
- dashboard:     http://localhost:7109
- customer:      http://localhost:7110
- disbursement:  http://localhost:7111
- auth:          http://localhost:7112
