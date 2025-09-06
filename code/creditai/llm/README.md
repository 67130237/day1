# LLM Configs / Clients

ที่นี่เก็บคอนฟิกของ LLM สำหรับระบบ CreditAI (ใช้ OpenRouter เป็นหลัก)

## โครงสร้าง
- `openrouter/policy.md` — นโยบายโมเดลที่อนุญาต, rate limit, ค่าเริ่มต้น
- `model-maps.json` — mapping จาก router/agents/composer → model ids ที่จะใช้จริง

## วิธีใช้ (สรุป)
- ฝั่ง Orchestrator และ Agents อ่าน `model-maps.json` เพื่อเลือกโมเดล
- ฝั่ง Integrations.OpenRouter ใช้ค่าเริ่มต้นจาก appsettings แต่สามารถ override ตาม mapping นี้ได้
- หากเพิ่ม/เปลี่ยนโมเดล ต้องแก้ทั้ง `policy.md` และ `model-maps.json` ให้สอดคล้องกัน
