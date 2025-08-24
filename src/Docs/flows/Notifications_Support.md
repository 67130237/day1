# Feature Flow: Notifications & Support

## Flow Steps
1. ผู้ใช้เปิดกล่องแจ้งเตือน (`GET /api/v1/notifications`)
2. มาร์คอ่าน (`POST /api/v1/notifications/{id}/read`)
3. ผูก push token (`POST /api/v1/push/register`)
4. เปิด FAQ (`GET /api/v1/support/faq`)
5. สร้าง Ticket (`POST /api/v1/support/tickets`)
6. โต้ตอบ Ticket (`POST /api/v1/support/tickets/{id}/messages`)

## API Mapping
- `/notifications/*`
- `/support/*`

## HTTP Status
- 200 OK – สำเร็จ
- 201 Created – ticket ถูกสร้าง
- 404 Not Found – ticketId ไม่พบ