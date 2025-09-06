-- 002_create_stored_procs.sql
-- Example: read-only proc

USE CreditDb;
GO

IF OBJECT_ID('reporting.usp_GetCustomerSummary') IS NOT NULL DROP PROCEDURE reporting.usp_GetCustomerSummary;
GO
CREATE PROCEDURE reporting.usp_GetCustomerSummary
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) as TotalCustomers, AVG(MonthlyIncome) as AvgIncome FROM dbo.Customers;
END
GO
