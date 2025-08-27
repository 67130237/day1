# Bank Microservices API Spec (Full)
**Date:** 2025-08-27

ชุดเอกสารสเปก API สำหรับ Mobile Banking (minimal + disbursement + external: DOPA/OTP/CMS/AppSettings + Notification + Dashboard)  
ทุกบริการยึด Global Contract: headers (`xapp-trace-id`, `xapp-source`), error envelope (`{code,message,traceid}`).

## รายการไฟล์
- global-contract.md
- auth-service.md
- identity-service.md
- customer-service.md
- account-service.md
- transaction-service.md
- disbursement-service.md
- dopa-service.md
- otp-service.md
- cms-service.md
- appsettings-service.md
- notification-service.md
- dashboard-service.md

> หมายเหตุ: ทุก endpoint ระบุ schema รายฟิลด์ + ตัวอย่าง cURL/JSON + business errors + NFR
