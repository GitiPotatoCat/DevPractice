-- =============================================
-- Step 3: Insert Sample Data (500 rows for testing)
-- =============================================
USE ReinsuranceDB;


SET NOCOUNT ON;

DECLARE @i INT = 1;
DECLARE @TreatyTypes TABLE (Val NVARCHAR(50));
INSERT INTO @TreatyTypes VALUES ('Quota Share'), ('Surplus'), ('Excess of Loss'), ('Stop Loss'), ('Facultative');

DECLARE @LOB TABLE (Val NVARCHAR(50));
INSERT INTO @LOB VALUES ('Property'), ('Casualty'), ('Marine'), ('Aviation'), ('Life'), ('Engineering'), ('Motor');

DECLARE @Countries TABLE (Val NVARCHAR(60));
INSERT INTO @Countries VALUES ('USA'), ('UK'), ('Germany'), ('France'), ('Japan'), ('India'), ('Singapore'), ('UAE'), ('Switzerland'), ('Bermuda');

DECLARE @Currencies TABLE (Val NVARCHAR(10));
INSERT INTO @Currencies VALUES ('USD'), ('EUR'), ('GBP'), ('JPY'), ('INR'), ('CHF'), ('AED');

DECLARE @Statuses TABLE (Val NVARCHAR(20));
INSERT INTO @Statuses VALUES ('Active'), ('Expired'), ('Pending'), ('Cancelled');

DECLARE @Ceding TABLE (Val NVARCHAR(150));
INSERT INTO @Ceding VALUES ('AXA Insurance'), ('Allianz Global'), ('Zurich Insurance'), ('AIG'), ('Chubb Ltd'),
                          ('Tokio Marine'), ('Generali Group'), ('Lloyds of London'), ('Berkshire Hathaway'), ('MS&AD');

DECLARE @Reinsurers TABLE (Val NVARCHAR(150));
INSERT INTO @Reinsurers VALUES ('Munich Re'), ('Swiss Re'), ('Hannover Re'), ('SCOR'), ('Lloyds Re'),
                              ('Berkshire Re'), ('Everest Re'), ('PartnerRe'), ('RenaissanceRe'), ('Korean Re');

DECLARE @Underwriters TABLE (Val NVARCHAR(100));
INSERT INTO @Underwriters VALUES ('John Smith'), ('Maria Garcia'), ('David Chen'), ('Sarah Johnson'),
                                ('Michael Brown'), ('Emma Wilson'), ('Raj Patel'), ('Yuki Tanaka'),
                                ('Anna Mueller'), ('Carlos Silva');

WHILE @i <= 500
BEGIN
    DECLARE @tt NVARCHAR(50)  = (SELECT TOP 1 Val FROM @TreatyTypes ORDER BY NEWID());
    DECLARE @lobb NVARCHAR(50) = (SELECT TOP 1 Val FROM @LOB ORDER BY NEWID());
    DECLARE @cn NVARCHAR(60)  = (SELECT TOP 1 Val FROM @Countries ORDER BY NEWID());
    DECLARE @cur NVARCHAR(10) = (SELECT TOP 1 Val FROM @Currencies ORDER BY NEWID());
    DECLARE @st NVARCHAR(20)  = (SELECT TOP 1 Val FROM @Statuses ORDER BY NEWID());
    DECLARE @cd NVARCHAR(150) = (SELECT TOP 1 Val FROM @Ceding ORDER BY NEWID());
    DECLARE @re NVARCHAR(150) = (SELECT TOP 1 Val FROM @Reinsurers ORDER BY NEWID());
    DECLARE @uw NVARCHAR(100) = (SELECT TOP 1 Val FROM @Underwriters ORDER BY NEWID());
    DECLARE @incept DATE = DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 720), GETDATE());
    DECLARE @expiry DATE = DATEADD(YEAR, 1, @incept);

    INSERT INTO dbo.Treaties
    (TreatyCode, TreatyName, CedingCompany, Reinsurer, TreatyType, LineOfBusiness,
     Country, Currency, SumInsured, PremiumAmount, CommissionPct, RetentionPct,
     InceptionDate, ExpiryDate, Treaty_Status, UnderwriterName)
    VALUES
    ('TR-' + RIGHT('00000' + CAST(@i AS NVARCHAR(5)), 5),
     @lobb + ' Treaty ' + CAST(@i AS NVARCHAR(5)),
     @cd, @re, @tt, @lobb, @cn, @cur,
     CAST(ABS(CHECKSUM(NEWID())) % 90000000 + 1000000 AS DECIMAL(18,2)),
     CAST(ABS(CHECKSUM(NEWID())) % 900000 + 10000 AS DECIMAL(18,2)),
     CAST(ABS(CHECKSUM(NEWID())) % 30 + 5 AS DECIMAL(5,2)),
     CAST(ABS(CHECKSUM(NEWID())) % 60 + 20 AS DECIMAL(5,2)),
     @incept, @expiry, @st, @uw);

    SET @i = @i + 1;
END
GO

SELECT COUNT(*) AS TotalRows FROM dbo.Treaties;
SELECT TOP 10 * FROM dbo.Treaties;
GO