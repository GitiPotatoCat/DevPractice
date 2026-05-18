USE ReinsuranceDB
GO


IF OBJECT_ID('dbo.Treaties', 'U') IS NOT NULL
    DROP TABLE dbo.Treaties
GO


CREATE TABLE dbo.Treaties
(
    TreatyId            INT IDENTITY(1,1)   PRIMARY KEY,
    TreatyCode          NVARCHAR(20)        NOT NULL,
    TreatyName          NVARCHAR(150)       NOT NULL,
    CedingCompany       NVARCHAR(150)       NOT NULL,
    Reinsurer           NVARCHAR(150)       NOT NULL,
    TreatyType          NVARCHAR(50)        NOT NULL,       -- Quota Share, Surplus, XOL, Stop Loss
    LineOfBusiness      NVARCHAR(50)        NOT NULL,       -- Property, Casualty, Marine, Aviation, Life
    Country             NVARCHAR(60)        NOT NULL,
    Currency            NVARCHAR(10)        NOT NULL,
    SumInsured          DECIMAL(18,2)       NOT NULL,
    PremiumAmount       DECIMAL(18,2)       NOT NULL,
    CommissionPct       DECIMAL(5,2)        NOT NULL,
    RetentionPct        DECIMAL(5,2)        NOT NULL,
    InceptionDate       DATE                NOT NULL,
    ExpiryDate          DATE                NOT NULL,
    Treaty_Status       NVARCHAR(20)        NOT NULL,       -- Active, Expired, Pending, Cancelled
    UnderwriterName     NVARCHAR(100)       NOT NULL,
    CreatedDate         DATETIME            NOT NULL DEFAULT GETDATE()
);
GO
