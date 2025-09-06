-- 001_create_views.sql
-- Example: Create read-only reporting views for CreditAI

USE master;
IF DB_ID('CreditDb') IS NULL
    CREATE DATABASE CreditDb;
GO
USE CreditDb;
GO

-- Example table
IF OBJECT_ID('dbo.Customers') IS NULL
CREATE TABLE dbo.Customers (
    CustomerId INT PRIMARY KEY,
    FullName NVARCHAR(200),
    DateOfBirth DATE,
    MonthlyIncome DECIMAL(18,2)
);

-- Example view (RO)
IF OBJECT_ID('reporting.vw_Customers') IS NOT NULL DROP VIEW reporting.vw_Customers;
IF SCHEMA_ID('reporting') IS NULL EXEC('CREATE SCHEMA reporting');
GO
CREATE VIEW reporting.vw_Customers AS
SELECT CustomerId, FullName, Year(getdate())-Year(DateOfBirth) as Age, MonthlyIncome
FROM dbo.Customers;
GO
