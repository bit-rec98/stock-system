-- Creates the application database and a least-privilege login for the API.
-- Run by the `db-init` service (docker compose) with: sqlcmd -v APP_PASSWORD=... -i init.sql
-- The login can manage its own database schema (EF migrations) but nothing else on the server.
IF DB_ID(N'StockSystemDb') IS NULL
    CREATE DATABASE [StockSystemDb];
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'stock_app')
    CREATE LOGIN [stock_app] WITH PASSWORD = N'$(APP_PASSWORD)', CHECK_POLICY = ON;
GO

USE [StockSystemDb];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'stock_app')
    CREATE USER [stock_app] FOR LOGIN [stock_app];
GO

ALTER ROLE db_owner ADD MEMBER [stock_app];
GO
