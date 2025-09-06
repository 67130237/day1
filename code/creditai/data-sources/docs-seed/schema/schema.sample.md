# Logical Schema (ตัวอย่าง)

## Tables
- **dbo.Customers**
  - `CustomerId` (PK), `NationalId`, `FullName`, `DateOfBirth`, `MonthlyIncome`, `CreatedAt`

- **dbo.Contracts**
  - `ContractNo` (PK), `CustomerId` (FK), `ProductCode`, `Principal`, `InterestRatePct`, `TermMonths`, `StartDate`, `Status`, `OutstandingBalance`, `CreatedAt`

- **dbo.Payments**
  - `Id` (PK), `ContractNo` (FK), `PaidAt`, `Amount`, `Channel`, `CreatedAt`

## Reporting Views
- **reporting.vw_Customers**
- **reporting.vw_Contracts**
- **reporting.vw_Payments**
