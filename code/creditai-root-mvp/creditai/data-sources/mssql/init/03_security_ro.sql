-- 03_security_ro.sql
USE CreditAI;
GO

-- Grant read-only access to reporting views only
GRANT SELECT ON SCHEMA::reporting TO creditai_ro_user;
GO

-- Explicitly deny writes
DENY INSERT, UPDATE, DELETE ON SCHEMA::reporting TO creditai_ro_user;
DENY CONTROL, ALTER, REFERENCES ON SCHEMA::reporting TO creditai_ro_user;
GO

-- Optional: lock base tables for this user
DENY SELECT ON SCHEMA::dbo TO creditai_ro_user;
GO
