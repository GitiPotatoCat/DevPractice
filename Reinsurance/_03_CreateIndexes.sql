USE ReinsuranceDB


SELECT * FROM dbo.Treaties



CREATE  INDEX   IX_Treaties_TreatyType      ON      dbo.Treaties(TreatyType);
CREATE  INDEX   IX_Treaties_LineOfBusiness  ON      dbo.Treaties(LineOfBusiness);
CREATE  INDEX   IX_Treaties_Country         ON      dbo.Treaties(Country);
CREATE  INDEX   IX_Treaties_Status          ON      dbo.Treaties(Treaty_Status);
GO