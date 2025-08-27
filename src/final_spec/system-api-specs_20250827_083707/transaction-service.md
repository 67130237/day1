# transaction-service @ 1.0.0

> **Base Path**: `/xapi/v1`  
> **xapp-source**: `transaction-service@1.0.0`

## POST /xapi/v1/transfers/quote
คำนวณค่าธรรมเนียม/ตรวจวงเงิน/AML

### Request
| Field | Type | Req |
|---|---|---|
| sourceAccountId | string | Y |
| dest | object | Y | { "accountNo":"...", "bankCode":"...", "name":"..." } |
| amount | string | Y |
| currency | string | Y |

### Response 200
```json
{ "fee":"10.00","currency":"THB","limitOk":true,"amlFlag":false,"message":"" }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| TXN-QUOTE-VAL | 400 | ข้อมูลไม่ถูกต้อง |
| TXN-QUOTE-LIMIT | 409 | เกินวงเงิน |
| TXN-QUOTE-SYS | 500 | ระบบขัดข้อง |

---

## POST /xapi/v1/transfers
ยืนยันทำรายการโอน

### Request
| Field | Type | Req |
|---|---|---|
| sourceAccountId | string | Y |
| dest | object | Y |
| amount | string | Y |
| currency | string | Y |
| note | string | N |
| idempotencyKey | string | Y | ผ่าน header `Idempotency-Key` |

### Response 201
```json
{ "transferId":"...", "status":"SUCCESS", "postedAt":"ISO8601" }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| TXN-XFER-NOFUND | 409 | ยอดเงินไม่พอ |
| TXN-XFER-AML | 409 | ถูกบล็อกตาม AML |
| TXN-XFER-DUP | 409 | ทำซ้ำ (Idempotent) |
| TXN-XFER-SYS | 500 | ระบบขัดข้อง |

---

## POST /xapi/v1/withdrawals
สร้างคำสั่งถอน

### Request
| Field | Type | Req |
|---|---|---|
| sourceAccountId | string | Y |
| amount | string | Y |
| currency | string | Y |
| channel | enum | Y | atm \| counter |
| idempotencyKey | string | Y | header |

### Response 201
```json
{ "withdrawalId":"...", "status":"issued", "token":"ABC123", "expireAt":"ISO8601" }
```

### Errors
| Code | HTTP | Message |
|---|---|---|
| TXN-WD-NOFUND | 409 | ยอดเงินไม่พอ |
| TXN-WD-LIMIT | 409 | เกินวงเงิน |
| TXN-WD-SYS | 500 | ระบบขัดข้อง |
