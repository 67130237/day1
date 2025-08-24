# Feature Flow: Register & Login

## Flow Steps
1. ผู้ใช้เปิดแอป → สมัครสมาชิก (`POST /api/v1/auth/register`)
2. ยืนยัน OTP (`POST /api/v1/auth/otp/verify`)
3. เข้าสู่ระบบ (`POST /api/v1/auth/login`)
4. ได้รับ JWT Access/Refresh token → ใช้งาน API อื่น ๆ

## API Mapping
- **POST /auth/register**
- **POST /auth/otp/verify**
- **POST /auth/login**
- **POST /auth/refresh**
- **POST /auth/logout**
- **GET /auth/devices**, **DELETE /auth/devices/{id}**

## HTTP Status
- 200 OK – สำเร็จ
- 201 Created – สร้างบัญชีใหม่
- 400 Bad Request – input ไม่ถูกต้อง
- 401 Unauthorized – credential ไม่ถูกต้อง
- 429 Too Many Requests – OTP เกิน limit