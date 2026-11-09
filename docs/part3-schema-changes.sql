/*
  Upgrade an existing database created with the original docs/RaceDayDB.sql
  so it matches the RaceDay.Api EF Core model.

  Back up the database first. Run once in SQL Server Management Studio.
  This preserves users, events, categories, enrolments and results.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- The API stores Run/Walk/Cycle directly instead of using the old EventTypes lookup.
IF COL_LENGTH('dbo.Events', 'EventType') IS NULL
BEGIN
    ALTER TABLE dbo.Events ADD EventType NVARCHAR(10) NULL;
    UPDATE e
      SET EventType = CASE
          WHEN et.TypeName LIKE '%walk%' THEN 'Walk'
          WHEN et.TypeName LIKE '%cycl%' OR et.TypeName LIKE '%bike%' THEN 'Cycle'
          ELSE 'Run'
      END
    FROM dbo.Events e
    LEFT JOIN dbo.EventTypes et ON et.EventTypeID = e.EventTypeID;

    ALTER TABLE dbo.Events ALTER COLUMN EventType NVARCHAR(10) NOT NULL;
    ALTER TABLE dbo.Events ADD CONSTRAINT CK_Events_EventType
      CHECK (EventType IN ('Run','Walk','Cycle'));
END;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Events_EventType')
    ALTER TABLE dbo.Events DROP CONSTRAINT FK_Events_EventType;
IF COL_LENGTH('dbo.Events', 'EventTypeID') IS NOT NULL
    ALTER TABLE dbo.Events DROP COLUMN EventTypeID;
IF OBJECT_ID('dbo.EventTypes', 'U') IS NOT NULL DROP TABLE dbo.EventTypes;

-- Newer optional fields used by the API.
IF COL_LENGTH('dbo.Events', 'BannerUrl') IS NULL
    ALTER TABLE dbo.Events ADD BannerUrl NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.Users', 'ProfilePictureUrl') IS NULL
    ALTER TABLE dbo.Users ADD ProfilePictureUrl NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.Categories', 'Description') IS NULL
    ALTER TABLE dbo.Categories ADD Description NVARCHAR(250) NULL;
IF COL_LENGTH('dbo.Categories', 'MinimumAge') IS NULL
    ALTER TABLE dbo.Categories ADD MinimumAge INT NULL;
IF COL_LENGTH('dbo.Categories', 'MaximumAge') IS NULL
    ALTER TABLE dbo.Categories ADD MaximumAge INT NULL;
IF COL_LENGTH('dbo.Results', 'RecordedAt') IS NULL
    ALTER TABLE dbo.Results ADD RecordedAt DATETIME2 NULL;

-- API models require values for these properties; provide safe values for legacy NULL rows.
UPDATE dbo.Events SET Distance = 1.00 WHERE Distance IS NULL;
ALTER TABLE dbo.Events ALTER COLUMN Distance DECIMAL(6,2) NOT NULL;
UPDATE dbo.Results SET FinishTime = '00:00:00' WHERE FinishTime IS NULL;
UPDATE dbo.Results SET Position = 1 WHERE Position IS NULL;
UPDATE dbo.Results SET RecordedAt = SYSUTCDATETIME() WHERE RecordedAt IS NULL;
ALTER TABLE dbo.Results ALTER COLUMN FinishTime TIME(0) NOT NULL;
ALTER TABLE dbo.Results ALTER COLUMN Position INT NOT NULL;
ALTER TABLE dbo.Results ALTER COLUMN RecordedAt DATETIME2 NOT NULL;
ALTER TABLE dbo.Results ADD CONSTRAINT DF_Results_RecordedAt
    DEFAULT SYSUTCDATETIME() FOR RecordedAt;

COMMIT TRANSACTION;
GO
