# System API Specs (Field-level)

สเปกต่อ service แยกไฟล์ `.md` อ้างอิงจาก flow ล่าสุด และทำให้สอดคล้องกับ GlobalContract
- รวม endpoint ที่ใช้จริงใน flow เท่านั้น (minimal, implementable)
- ทุกการตอบกลับผิดพลาดใช้ Error Envelope เดียวกัน พร้อม `code` ชัดเจน
- ใช้ `Idempotency-Key` กับคำสั่งที่ควรเป็น idempotent (transfers, withdrawals, disbursements)

> หากต้องการ JSON Schema (.json) สำหรับแต่ละ endpoint เพิ่มเติม แจ้งได้ครับ
