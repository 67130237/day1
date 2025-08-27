# Credit Service Database Schema (MSSQL)
*Version:* 1.0  
*Owner:* Research – Agentic AI + MCP + RAG  
*Last Updated:* 2025-08-26

> This document defines the **logical & physical schema** for the synthetic credit service dataset used in the thesis prototype. It includes ERD, DDL, constraints, security patterns (RLS + Dynamic Data Masking), views, seed examples, and reference queries.

---

## 1) Overview
The schema models three core entities: **Customer**, **Loan**, and **Payment**. It is designed to support:
- Analytical and operational queries for **credit service** tasks
- **Row‑Level Security (RLS)** and **Dynamic Data Masking (DDM)** for PII protection
- Query templates for use by **MCP MssqlTooling**

---

## 2) ER Diagram
```mermaid
erDiagram
  Customer ||--o{ Loan : has
  Loan ||--o{ Payment : has

  Customer {
    INT CustomerId PK
    NVARCHAR(20) CitizenId UNIQUE
    NVARCHAR(200) FullName
    NVARCHAR(50) Segment
    NVARCHAR(30) Phone
    NVARCHAR(200) Email
    DATETIME2 CreatedAt
  }

  Loan {
    INT LoanId PK
    INT CustomerId FK -> Customer.CustomerId
    NVARCHAR(50) ProductCode
    DECIMAL(18,2) Principal
    DECIMAL(5,2) InterestRate
    DATE StartDate
    DATE MaturityDate
    NVARCHAR(30) Status
  }

  Payment {
    INT PaymentId PK
    INT LoanId FK -> Loan.LoanId
    DATE PayDate
    DECIMAL(18,2) Amount
    NVARCHAR(50) Channel
    NVARCHAR(200) Remark
  }
```
> Notes: `Status` in **Loan** typically takes values: `Active`, `Closed`, `Default`.

---

## 3) DDL – Tables
```sql
-- 3.1 Customer
CREATE TABLE dbo.Customer (
  CustomerId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  CitizenId NVARCHAR(20) NOT NULL UNIQUE,
  FullName NVARCHAR(200) NOT NULL,
  Segment NVARCHAR(50) NULL,            -- e.g., Retail, SME, VIP
  Phone NVARCHAR(30) NULL,
  Email NVARCHAR(200) NULL,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- 3.2 Loan
CREATE TABLE dbo.Loan (
  LoanId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  CustomerId INT NOT NULL,
  ProductCode NVARCHAR(50) NOT NULL,    -- e.g., PERS, AUTO, HOME
  Principal DECIMAL(18,2) NOT NULL,
  InterestRate DECIMAL(5,2) NOT NULL,   -- percent, e.g., 12.50
  StartDate DATE NOT NULL,
  MaturityDate DATE NULL,
  Status NVARCHAR(30) NOT NULL,
  CONSTRAINT FK_Loan_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer(CustomerId)
);

-- 3.3 Payment
CREATE TABLE dbo.Payment (
  PaymentId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  LoanId INT NOT NULL,
  PayDate DATE NOT NULL,
  Amount DECIMAL(18,2) NOT NULL,
  Channel NVARCHAR(50) NULL, -- counter, app, transfer
  Remark NVARCHAR(200) NULL,
  CONSTRAINT FK_Payment_Loan FOREIGN KEY (LoanId) REFERENCES dbo.Loan(LoanId)
);
```

---

## 4) Indexes
```sql
-- Customer
CREATE UNIQUE INDEX UX_Customer_CitizenId ON dbo.Customer(CitizenId);
CREATE INDEX IX_Customer_Segment ON dbo.Customer(Segment);

-- Loan
CREATE INDEX IX_Loan_CustomerId ON dbo.Loan(CustomerId);
CREATE INDEX IX_Loan_Status ON dbo.Loan(Status);
CREATE INDEX IX_Loan_StartDate ON dbo.Loan(StartDate);

-- Payment
CREATE INDEX IX_Payment_LoanId ON dbo.Payment(LoanId);
CREATE INDEX IX_Payment_PayDate ON dbo.Payment(PayDate);
```

---

## 5) Security
### 5.1 Dynamic Data Masking (DDM)
> Mask PII at query time for non‑privileged users.
```sql
ALTER TABLE dbo.Customer ALTER COLUMN CitizenId ADD MASKED WITH (FUNCTION = 'partial(2,"XXXXXX",4)');
ALTER TABLE dbo.Customer ALTER COLUMN Phone     ADD MASKED WITH (FUNCTION = 'partial(0,"********",2)');
ALTER TABLE dbo.Customer ALTER COLUMN Email     ADD MASKED WITH (FUNCTION = 'email()');
```

> To **exclude** specific logins/roles from masking: grant `UNMASK` as needed.
```sql
-- Example: allow Auditor to see real data
GRANT UNMASK TO [role_Auditor];
```

### 5.2 Row‑Level Security (RLS)
> Restrict rows based on the user’s role/claim (e.g., segment).

```sql
-- (1) Security predicate function
CREATE SCHEMA Sec;
GO
CREATE FUNCTION Sec.fnCustomerAccessPredicate(@Segment AS NVARCHAR(50))
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS fn_result
WHERE
  -- Example rule: user context holds allowed segment in SESSION_CONTEXT
  @Segment = CAST(SESSION_CONTEXT(N'AllowedSegment') AS NVARCHAR(50))
  OR CAST(SESSION_CONTEXT(N'IsAdmin') AS NVARCHAR(5)) = 'true';
GO

-- (2) Security policy on Customer
CREATE SECURITY POLICY Sec.CustomerRLS
ADD FILTER PREDICATE Sec.fnCustomerAccessPredicate(Segment) ON dbo.Customer,
ADD BLOCK PREDICATE  Sec.fnCustomerAccessPredicate(Segment) ON dbo.Customer
WITH (STATE = ON);
GO

-- (3) Propagate RLS via views or join predicates
-- Example: for Loan, join to Customer to inherit segment policy in views
```

> Set context per session for app users:
```sql
EXEC sys.sp_set_session_context @key = N'AllowedSegment', @value = N'Retail';
EXEC sys.sp_set_session_context @key = N'IsAdmin', @value = N'false';
```

---

## 6) Views (for MCP Allowlist / Safer Query Surface)
```sql
CREATE VIEW dbo.vwCustomerPublic
AS
SELECT
  CustomerId,
  CitizenId, -- subject to DDM
  FullName,
  Segment,
  CreatedAt
FROM dbo.Customer;
GO

CREATE VIEW dbo.vwLoanDetail
AS
SELECT
  l.LoanId, l.CustomerId, c.FullName,
  l.ProductCode, l.Principal, l.InterestRate,
  l.StartDate, l.MaturityDate, l.Status
FROM dbo.Loan l
JOIN dbo.Customer c ON c.CustomerId = l.CustomerId;
GO

CREATE VIEW dbo.vwPaymentSummary
AS
SELECT
  p.LoanId,
  COUNT(*) AS TxnCount,
  SUM(p.Amount) AS TotalPaid,
  MIN(p.PayDate) AS FirstPayDate,
  MAX(p.PayDate) AS LastPayDate
FROM dbo.Payment p
GROUP BY p.LoanId;
GO
```

---

## 7) Seed Data (Example)
```sql
-- Customers
INSERT dbo.Customer (CitizenId, FullName, Segment, Phone, Email)
VALUES
(N'110170000001', N'Somchai Prasert', N'Retail', N'0891112222', N'somchai@example.com'),
(N'110170000002', N'Supaporn Inta',   N'VIP',    N'0891113333', N'supaporn@example.com');

-- Loans
INSERT dbo.Loan (CustomerId, ProductCode, Principal, InterestRate, StartDate, MaturityDate, Status)
VALUES
(1, N'PERS', 150000.00, 15.50, '2024-01-10', '2026-01-10', N'Active'),
(1, N'AUTO', 420000.00, 6.95,  '2023-08-01', '2028-08-01', N'Active'),
(2, N'PERS', 80000.00,  18.00, '2023-02-01', '2025-02-01', N'Closed');

-- Payments
INSERT dbo.Payment (LoanId, PayDate, Amount, Channel, Remark)
VALUES
(1, '2024-02-10', 5000.00, N'app',     N'normal'),
(1, '2024-03-10', 5000.00, N'transfer',N'normal'),
(2, '2023-09-01', 7000.00, N'counter', N'normal'),
(3, '2023-03-01', 4000.00, N'app',     N'final');
```

---

## 8) Reference Queries (Templates)
```sql
-- Q1: Outstanding principal by customer (simplified; assumes principal as base)
SELECT c.FullName, SUM(l.Principal) AS TotalPrincipal
FROM dbo.Customer c
JOIN dbo.Loan l ON l.CustomerId = c.CustomerId
WHERE c.CitizenId = @CitizenId
  AND l.Status = N'Active'
GROUP BY c.FullName;

-- Q2: New loans opened in a month
SELECT COUNT(*) AS NewLoans
FROM dbo.Loan
WHERE StartDate >= @StartDate AND StartDate < DATEADD(MONTH, 1, @StartDate);

-- Q3: Customers with delinquency > N days (example heuristic via last payment)
SELECT c.FullName, l.LoanId,
  DATEDIFF(DAY, ISNULL(ps.LastPayDate, l.StartDate), GETDATE()) AS DaysSinceLastPay
FROM dbo.Loan l
JOIN dbo.Customer c ON c.CustomerId = l.CustomerId
LEFT JOIN (
  SELECT LoanId, MAX(PayDate) AS LastPayDate
  FROM dbo.Payment GROUP BY LoanId
) ps ON ps.LoanId = l.LoanId
WHERE l.Status = N'Active'
  AND DATEDIFF(DAY, ISNULL(ps.LastPayDate, l.StartDate), GETDATE()) > @Days;
```

---

## 9) MCP Allowlist (Examples)
> Restrict MssqlTooling to call only these views / parameterized templates.
```sql
-- Example allowlist (store in app config or DB table)
--   name: loan.by_citizen
--   sql : SELECT * FROM dbo.vwLoanDetail WHERE CustomerId = @CustomerId;
--   params: @CustomerId INT

--   name: payment.summary.by_loan
--   sql : SELECT * FROM dbo.vwPaymentSummary WHERE LoanId = @LoanId;
--   params: @LoanId INT
```

---

## 10) Change Log
| Version | Date       | Author         | Changes |
|--------:|------------|----------------|---------|
| 1.0     | 2025-08-26 | P. Yensiri     | Initial release (Customer/Loan/Payment, RLS/DDM, views, seeds) |
