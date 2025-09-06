# Data Dictionary (ตัวอย่าง)

| Field | Table/View | Type | Description |
|------|------------|------|-------------|
| CustomerId | dbo.Customers | INT | รหัสลูกค้า (PK) |
| NationalIdMasked | reporting.vw_Customers | VARCHAR | บัตร ปชช. (ปกปิด) |
| ContractNo | dbo.Contracts | VARCHAR(32) | เลขที่สัญญา |
| OutstandingBalance | dbo.Contracts | DECIMAL | ยอดเงินคงค้าง |
| PaidAt | dbo.Payments | DATE | วันที่ชำระ |
| Amount | dbo.Payments | DECIMAL | จำนวนเงินที่ชำระ |
