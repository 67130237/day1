# Feature Flow: Profile, CMS, Analytics, Admin

## Profile
- ดู/แก้ไขโปรไฟล์ (`GET/PUT /api/v1/me`)
- จัดการบัญชีธนาคาร (`GET/POST/DELETE /api/v1/me/banks`)
- ดูเอกสาร (`GET /api/v1/documents`)

## CMS
- แบนเนอร์ (`GET /api/v1/cms/banners`)
- บทความ (`GET /api/v1/cms/articles`)

## Analytics
- การชำระคืน (`GET /api/v1/analytics/repayment`)
- Credit Health (`GET /api/v1/credit/health`)

## Admin
- ดูใบสมัคร (`GET /api/v1/admin/applications`)
- อนุมัติ/ปฏิเสธ (`POST /api/v1/admin/applications/{id}/approve`)
- Disburse Loan (`POST /api/v1/admin/loans/{id}/disburse`)

## HTTP Status
- 200 OK – สำเร็จ
- 201 Created – resource ใหม่
- 400 Bad Request – input ไม่ถูกต้อง
- 401 Unauthorized – ไม่มีสิทธิ์
- 403 Forbidden – ถูกปฏิเสธสิทธิ์
- 404 Not Found – ไม่พบ resource