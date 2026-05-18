-- =============================================================
-- 03_seed_footballclubs.sql
-- Seeds dbo.FootballClubs with ~5,000 deterministic rows.
-- Re-runnable: clears the table first.
-- =============================================================

USE FootballDb;
GO

-- Wipe and reseed so this script is idempotent.
TRUNCATE TABLE dbo.FootballClubs;
GO

-- ----- 1) Insert ~50 real clubs as anchor data ----------------
INSERT INTO dbo.FootballClubs (ClubName, Country, League, FoundedYear, Stadium, TitlesWon) VALUES
('Real Madrid',         'Spain',       'La Liga',           1902, 'Santiago Bernabeu',          35),
('FC Barcelona',        'Spain',       'La Liga',           1899, 'Spotify Camp Nou',           27),
('Atletico Madrid',     'Spain',       'La Liga',           1903, 'Riyadh Air Metropolitano',   11),
('Sevilla FC',          'Spain',       'La Liga',           1890, 'Ramon Sanchez Pizjuan',       1),
('Valencia CF',         'Spain',       'La Liga',           1919, 'Mestalla',                    6),
('Manchester United',   'England',     'Premier League',    1878, 'Old Trafford',               20),
('Liverpool FC',        'England',     'Premier League',    1892, 'Anfield',                    19),
('Manchester City',     'England',     'Premier League',    1880, 'Etihad Stadium',              8),
('Chelsea FC',          'England',     'Premier League',    1905, 'Stamford Bridge',             6),
('Arsenal FC',          'England',     'Premier League',    1886, 'Emirates Stadium',           13),
('Tottenham Hotspur',   'England',     'Premier League',    1882, 'Tottenham Hotspur Stadium',   2),
('Newcastle United',    'England',     'Premier League',    1892, 'St James'' Park',             4),
('Bayern Munich',       'Germany',     'Bundesliga',        1900, 'Allianz Arena',              33),
('Borussia Dortmund',   'Germany',     'Bundesliga',        1909, 'Signal Iduna Park',           8),
('RB Leipzig',          'Germany',     'Bundesliga',        2009, 'Red Bull Arena',              0),
('Bayer Leverkusen',    'Germany',     'Bundesliga',        1904, 'BayArena',                    1),
('Eintracht Frankfurt', 'Germany',     'Bundesliga',        1899, 'Deutsche Bank Park',          1),
('Juventus',            'Italy',       'Serie A',           1897, 'Allianz Stadium',            36),
('Inter Milan',         'Italy',       'Serie A',           1908, 'San Siro',                   20),
('AC Milan',            'Italy',       'Serie A',           1899, 'San Siro',                   19),
('AS Roma',             'Italy',       'Serie A',           1927, 'Stadio Olimpico',             3),
('SSC Napoli',          'Italy',       'Serie A',           1926, 'Diego Armando Maradona',      3),
('Lazio',               'Italy',       'Serie A',           1900, 'Stadio Olimpico',             2),
('Paris Saint-Germain', 'France',      'Ligue 1',           1970, 'Parc des Princes',           12),
('Olympique Marseille', 'France',      'Ligue 1',           1899, 'Stade Velodrome',             9),
('Olympique Lyonnais',  'France',      'Ligue 1',           1950, 'Groupama Stadium',            7),
('AS Monaco',           'France',      'Ligue 1',           1924, 'Stade Louis II',              8),
('Lille OSC',           'France',      'Ligue 1',           1944, 'Stade Pierre-Mauroy',         4),
('Ajax',                'Netherlands', 'Eredivisie',        1900, 'Johan Cruijff ArenA',        36),
('PSV Eindhoven',       'Netherlands', 'Eredivisie',        1913, 'Philips Stadion',            25),
('Feyenoord',           'Netherlands', 'Eredivisie',        1908, 'De Kuip',                    16),
('Benfica',             'Portugal',    'Primeira Liga',     1904, 'Estadio da Luz',             38),
('FC Porto',            'Portugal',    'Primeira Liga',     1893, 'Estadio do Dragao',          30),
('Sporting CP',         'Portugal',    'Primeira Liga',     1906, 'Estadio Jose Alvalade',      20),
('Celtic FC',           'Scotland',    'Scottish Prem.',    1887, 'Celtic Park',                54),
('Rangers FC',          'Scotland',    'Scottish Prem.',    1872, 'Ibrox Stadium',              55),
('Galatasaray',         'Turkey',      'Super Lig',         1905, 'Rams Park',                  25),
('Fenerbahce',          'Turkey',      'Super Lig',         1907, 'Sukru Saracoglu',            19),
('Besiktas',            'Turkey',      'Super Lig',         1903, 'Tupras Stadium',             16),
('Club Brugge',         'Belgium',     'Pro League',        1891, 'Jan Breydel Stadium',        19),
('Anderlecht',          'Belgium',     'Pro League',        1908, 'Lotto Park',                 34),
('Shakhtar Donetsk',    'Ukraine',     'Premier Liha',      1936, 'Arena Lviv',                 14),
('Dynamo Kyiv',         'Ukraine',     'Premier Liha',      1927, 'Olimpiyskiy',                17),
('Red Star Belgrade',   'Serbia',      'SuperLiga',         1945, 'Rajko Mitic Stadium',        35),
('Olympiacos',          'Greece',      'Super League',      1925, 'Karaiskakis',                47),
('Panathinaikos',       'Greece',      'Super League',      1908, 'OAKA',                       20),
('Boca Juniors',        'Argentina',   'Liga Profesional',  1905, 'La Bombonera',               35),
('River Plate',         'Argentina',   'Liga Profesional',  1901, 'Mas Monumental',             38),
('Flamengo',            'Brazil',      'Brasileirao',       1895, 'Maracana',                    8),
('Palmeiras',           'Brazil',      'Brasileirao',       1914, 'Allianz Parque',             12);
GO

-- ----- 2) Generate ~5,000 synthetic clubs ---------------------
-- We use a tally table built from system views to avoid loops.

;WITH N AS (
    SELECT TOP (5000)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects a
    CROSS JOIN sys.all_objects b
),
Countries AS (
    SELECT * FROM (VALUES
        ('Spain','La Liga'), ('England','Premier League'), ('Germany','Bundesliga'),
        ('Italy','Serie A'), ('France','Ligue 1'), ('Portugal','Primeira Liga'),
        ('Netherlands','Eredivisie'), ('Belgium','Pro League'), ('Scotland','Scottish Prem.'),
        ('Turkey','Super Lig'), ('Greece','Super League'), ('Brazil','Brasileirao'),
        ('Argentina','Liga Profesional'), ('Mexico','Liga MX'), ('USA','MLS'),
        ('Japan','J1 League'), ('South Korea','K League 1'), ('Saudi Arabia','Saudi Pro League'),
        ('Australia','A-League'), ('Norway','Eliteserien'), ('Sweden','Allsvenskan'),
        ('Denmark','Superligaen'), ('Switzerland','Super League'), ('Austria','Bundesliga AT'),
        ('Czechia','Fortuna Liga'), ('Poland','Ekstraklasa')
    ) AS c(Country, League)
),
Words AS (
    SELECT * FROM (VALUES
        ('Athletic'),('Sporting'),('Real'),('Royal'),('United'),('City'),('Olympic'),
        ('Dynamo'),('Inter'),('Atletico'),('Rapid'),('Slavia'),('Sparta'),('Estrella'),
        ('Northern'),('Southern'),('Eastern'),('Western'),('Central'),('Lakeside'),
        ('Riverside'),('Mountain'),('Coastal'),('Highland'),('Lowland'),('Forest'),
        ('Iron'),('Golden'),('Silver'),('Crimson'),('Azure'),('Emerald')
    ) AS w(Word)
),
Cities AS (
    SELECT * FROM (VALUES
        ('Avalon'),('Brighton'),('Carlow'),('Drayton'),('Elford'),('Fairmont'),
        ('Glenwood'),('Harlow'),('Ironbridge'),('Jasper'),('Kingsford'),('Lockton'),
        ('Marlow'),('Norwood'),('Oakfield'),('Parkvale'),('Quinton'),('Riverton'),
        ('Saltwood'),('Thornbury'),('Underwood'),('Vinewood'),('Westbrook'),('Yardley'),
        ('Ashbourne'),('Barrowfield'),('Coldstream'),('Dunmore'),('Edgemont'),('Fenwick')
    ) AS ci(City)
)
INSERT INTO dbo.FootballClubs (ClubName, Country, League, FoundedYear, Stadium, TitlesWon)
SELECT
    -- Deterministic pseudo-random pick of one Word + one City
    (SELECT Word FROM Words ORDER BY (n.n * 31 + 7) % 32 OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY)
        + N' ' +
    (SELECT City FROM Cities ORDER BY (n.n * 17 + 3) % 30 OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY)
        + N' FC ' + CAST(n.n AS NVARCHAR(10))                                   AS ClubName,
    c.Country,
    c.League,
    1880 + (ABS(CHECKSUM(NEWID())) % 145)                                       AS FoundedYear,
    N'Stadium ' + CAST(n.n AS NVARCHAR(10))                                     AS Stadium,
    ABS(CHECKSUM(NEWID())) % 40                                                 AS TitlesWon
FROM N
CROSS APPLY (
    SELECT TOP (1) Country, League
    FROM Countries
    ORDER BY (n.n * 13 + 1) % 26
) c;
GO

-- ----- 3) Verify ----------------------------------------------
SELECT COUNT(*) AS TotalRows FROM dbo.FootballClubs;

SELECT TOP (10) Id, ClubName, Country, League, FoundedYear, Stadium, TitlesWon
FROM dbo.FootballClubs
ORDER BY Id;
GO