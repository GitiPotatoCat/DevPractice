-- =============================================================
-- 02_create_table_footballclubs.sql
-- Creates the FootballClubs table inside FootballDb.
-- =============================================================

USE FootballDb;
GO

IF OBJECT_ID('dbo.FootballClubs', 'U') IS NOT NULL
BEGIN
    PRINT 'Table dbo.FootballClubs already exists. Skipping.';
    RETURN;
END
GO

CREATE TABLE dbo.FootballClubs
(
    Id           INT             IDENTITY(1,1) NOT NULL,
    ClubName     NVARCHAR(150)   NOT NULL,
    Country      NVARCHAR(80)    NOT NULL,
    League       NVARCHAR(100)   NOT NULL,
    FoundedYear  INT             NOT NULL,
    Stadium      NVARCHAR(150)   NOT NULL,
    TitlesWon    INT             NOT NULL CONSTRAINT DF_FootballClubs_TitlesWon DEFAULT(0),

    CONSTRAINT PK_FootballClubs PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_FootballClubs_FoundedYear CHECK (FoundedYear BETWEEN 1800 AND 2100),
    CONSTRAINT CK_FootballClubs_TitlesWon  CHECK (TitlesWon >= 0)
);
GO

-- Helpful indexes for the columns we will sort/filter on most.
CREATE INDEX IX_FootballClubs_ClubName    ON dbo.FootballClubs(ClubName);
CREATE INDEX IX_FootballClubs_Country     ON dbo.FootballClubs(Country);
CREATE INDEX IX_FootballClubs_League      ON dbo.FootballClubs(League);
CREATE INDEX IX_FootballClubs_TitlesWon   ON dbo.FootballClubs(TitlesWon);
CREATE INDEX IX_FootballClubs_FoundedYear ON dbo.FootballClubs(FoundedYear);
GO

PRINT 'Table dbo.FootballClubs created with indexes.';
GO