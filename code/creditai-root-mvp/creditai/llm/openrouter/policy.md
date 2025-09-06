# OpenRouter Policy

## Allowed Models
- `openai/gpt-4o-mini` — **default** สำหรับ routing / intent classification / งานทั่วไป
- `openai/gpt-4o` — เหมาะกับ reasoning ลึก (Composer, ReAct)
- `openai/text-embedding-3-small` — embeddings (ingestion + RAG)
- `openai/text-embedding-3-large` — (ทางเลือก) ความละเอียดสูงกว่า

> หมายเหตุ: ชื่อโมเดลใน OpenRouter อาจต้องระบุผู้ให้บริการ เช่น `openai/gpt-4o`. ปรับให้ตรงกับบัญชี/plan ของคุณ

## Rate Limits (แนะนำ)
- Default: 60 RPM ต่อ client
- Max concurrency: 5
- Ingestion (embeddings): จำกัด batch ≤ 100 texts/นาที

## Defaults
- Chat/Composer: `openai/gpt-4o-mini`
- Embeddings: `openai/text-embedding-3-small`

## ข้อกำหนด
- ห้ามเรียกใช้โมเดลที่ไม่ได้อยู่ในรายการ Allowed Models ข้างต้น (เว้นแต่จะอัปเดต policy นี้)
- การเพิ่มโมเดลใหม่ ให้แก้ทั้ง `policy.md` และ `model-maps.json`
