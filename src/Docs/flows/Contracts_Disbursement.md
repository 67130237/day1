# Feature Flow: Contract & Disbursement

## Flow Steps
1. หลังผู้ใช้ accept offer → แสดงสัญญา (`GET /api/v1/contracts/{id}`)
2. ผู้ใช้ eSign (`POST /api/v1/contracts/{id}/esign`)
3. สัญญา active → รอ disbursement (`GET /api/v1/disbursements/{id}`)

## API Mapping
- `/contracts/*`
- `/disbursements/{contractId}`

## HTTP Status
- 200 OK – ข้อมูลสัญญา
- 202 Accepted – ระหว่างโอนเงิน
- 404 Not Found – contractId ไม่ถูกต้อง
- 500 Internal Server Error – โอนเงินล้มเหลว