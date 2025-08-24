# Feature Flow: Loan Application

## Flow Steps
1. ผู้ใช้เลือกสินเชื่อ (`GET /api/v1/loan-products`)
2. คำนวณค่างวด (`POST /api/v1/loan-calculator`)
3. ยื่นใบสมัคร (`POST /api/v1/loan-applications`)
4. อัปโหลดเอกสาร (`POST /api/v1/loan-applications/{id}/documents`)
5. Submit เข้าระบบตรวจ (`POST /api/v1/loan-applications/{id}/submit`)
6. ติดตามสถานะ (`GET /api/v1/loan-applications/{id}/status`)
7. ได้ offer → ตัดสินใจ accept/reject

## API Mapping
- `/loan-products`, `/loan-calculator`
- `/loan-applications` (CRUD & submit, status, documents, accept-offer, reject-offer)

## HTTP Status
- 201 Created – สมัครสำเร็จ
- 202 Accepted – เข้าสู่ under review
- 400 Bad Request – ข้อมูลไม่ครบ
- 409 Conflict – ส่งซ้ำโดยไม่มี Idempotency-Key
- 404 Not Found – applicationId ไม่ถูกต้อง