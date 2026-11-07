-- RaceDay Part 3 schema changes (Azure Blob Storage image URLs)
-- Run this ONLY if you created the database from a SQL script. If you use EF Core migrations, run:
--   dotnet ef migrations add Part3_ImageUrls --project RaceDay.Api
--   dotnet ef database update --project RaceDay.Api
-- Table names follow the EF Core model used by RaceDay.Api (Users, Events).

IF COL_LENGTH('dbo.Events', 'BannerUrl') IS NULL
    ALTER TABLE dbo.Events ADD BannerUrl NVARCHAR(500) NULL;
GO

IF COL_LENGTH('dbo.Users', 'ProfilePictureUrl') IS NULL
    ALTER TABLE dbo.Users ADD ProfilePictureUrl NVARCHAR(500) NULL;
GO
