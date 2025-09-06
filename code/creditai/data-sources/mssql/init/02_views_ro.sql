-- 02_views_ro.sql
USE CreditAI;
GO

-- Mask some PII fields and expose read-only reporting views
IF OBJECT_ID('reporting.vw_Customers') IS NULL
EXEC('
CREATE VIEW reporting.vw_Customers AS
SELECT
  c.CustomerId,
  LEFT(ISNULL(c.NationalId, ''*************''), 2) + REPLICATE(''*'', 9) + RIGHT(ISNULL(c.NationalId, ''*************''), 2) AS NationalIdMasked,
  c.FullName,
  c.DateOfBirth,
  c.MonthlyIncome,
  c.CreatedAt
FROM dbo.Customers c
');
GO

IF OBJECT_ID('reporting.vw_Contracts') IS NULL
EXEC('
CREATE VIEW reporting.vw_Contracts AS
SELECT
  k.ContractNo,
  k.CustomerId,
  k.ProductCode,
  k.Principal,
  k.InterestRatePct,
  k.TermMonths,
  k.StartDate,
  k.Status,
  k.OutstandingBalance,
  k.CreatedAt
FROM dbo.Contracts k
');
GO

IF OBJECT_ID('reporting.vw_Payments') IS NULL
EXEC('
CREATE VIEW reporting.vw_Payments AS
SELECT
  p.Id,
  p.ContractNo,
  p.PaidAt,
  p.Amount,
  p.Channel,
  p.CreatedAt
FROM dbo.Payments p
');
GO
