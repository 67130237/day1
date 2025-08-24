# Feature Flow: Loan Management

## Flow Steps
1. ผู้ใช้เข้าหน้า "My Loans" (`GET /api/v1/loans`)
2. ดูรายละเอียด (`GET /api/v1/loans/{id}`)
3. ดูตารางผ่อน (`GET /api/v1/loans/{id}/schedule`)
4. ดูประวัติธุรกรรม (`GET /api/v1/loans/{id}/transactions`)
5. ขอ Early/Partial Quote (`POST /api/v1/loans/{id}/early-quote`)

## API Mapping
- `/loans/*`

## HTTP Status
- 200 OK – สำเร็จ
- 202 Accepted – รอพิจารณาปรับโครงสร้างหนี้
- 404 Not Found – loanId ไม่ถูกต้อง