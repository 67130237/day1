-- 00_create_db.sql
-- Dev-only: create sample DB and principal
IF DB_ID('CreditAI') IS NULL
BEGIN
  CREATE DATABASE CreditAI;
END
GO

USE CreditAI;
GO

-- Create login & users (dev):
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'creditai_ro_login')
BEGIN
  CREATE LOGIN creditai_ro_login WITH PASSWORD = 'StrongPassword1!';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'creditai_ro_user')
BEGIN
  CREATE USER creditai_ro_user FOR LOGIN creditai_ro_login;
END
GO

-- Create schema for reporting
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'reporting')
BEGIN
  EXEC('CREATE SCHEMA reporting');
END
GO
