USE FootballDb;
GO

-- Total rows
SELECT COUNT(*) AS TotalClubs FROM dbo.FootballClubs;

-- Distribution by country
SELECT Country, COUNT(*) AS Clubs
FROM dbo.FootballClubs
GROUP BY Country
ORDER BY Clubs DESC;

-- Spot-check pagination math: page 3 of 20 per page, ordered by ClubName ASC
SELECT Id, ClubName, Country, League, FoundedYear, Stadium, TitlesWon
FROM dbo.FootballClubs
ORDER BY ClubName ASC
OFFSET 40 ROWS FETCH NEXT 20 ROWS ONLY;
GO