-- =============================================================
-- 01_create_database.sql
-- Creates the FootballDb database if it does not already exist.
-- =============================================================

IF DB_ID('FootballDb') IS NULL
BEGIN
    CREATE DATABASE FootballDb;
    PRINT 'Database FootballDb created.';
END
ELSE
BEGIN
    PRINT 'Database FootballDb already exists. Skipping.';
END
GO