# Feature Flow: Billing & Payments

## Flow Steps
1. ผู้ใช้ดูใบแจ้งหนี้ (`GET /api/v1/billing/{loanId}/invoices`)
2. เริ่มการชำระ (`POST /api/v1/payments/intent` หรือ `POST /api/v1/payments/charge`)
3. รอผล PSP callback (`POST /api/v1/webhooks/payments/provider`)
4. ตรวจสอบสถานะ (`GET /api/v1/payments/{id}`)
5. ดาวน์โหลดใบเสร็จ (`GET /api/v1/payments/{id}/receipt`)

## API Mapping
- `/billing/*`
- `/payments/*`
- `/webhooks/payments/provider`

## HTTP Status
- 200 OK – ชำระสำเร็จ
- 202 Accepted – payment pending
- 400 Bad Request – method ไม่รองรับ
- 402 Payment Required – ยอดเงินไม่พอ
- 409 Conflict – duplicate payment
- 500 Internal Server Error – PSP error