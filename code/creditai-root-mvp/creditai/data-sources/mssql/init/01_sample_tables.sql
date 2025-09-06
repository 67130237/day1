-- 01_sample_tables.sql
USE CreditAI;
GO

IF OBJECT_ID('dbo.Customers') IS NULL
BEGIN
  CREATE TABLE dbo.Customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    NationalId VARCHAR(13) NULL,
    FullName NVARCHAR(200) NOT NULL,
    DateOfBirth DATE NULL,
    MonthlyIncome DECIMAL(18,2) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
  );
END
GO

IF OBJECT_ID('dbo.Contracts') IS NULL
BEGIN
  CREATE TABLE dbo.Contracts (
    ContractNo VARCHAR(32) PRIMARY KEY,
    CustomerId INT NOT NULL,
    ProductCode VARCHAR(32) NOT NULL,
    Principal DECIMAL(18,2) NOT NULL,
    InterestRatePct DECIMAL(9,4) NOT NULL,
    TermMonths INT NOT NULL,
    StartDate DATE NOT NULL,
    Status VARCHAR(16) NOT NULL,
    OutstandingBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Contracts_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId)
  );
END
GO

IF OBJECT_ID('dbo.Payments') IS NULL
BEGIN
  CREATE TABLE dbo.Payments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ContractNo VARCHAR(32) NOT NULL,
    PaidAt DATE NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Channel VARCHAR(32) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Payments_Contracts FOREIGN KEY (ContractNo) REFERENCES dbo.Contracts(ContractNo)
  );
END
GO
