/* ============================================================
   RaceDay System - Database Schema & Seed Data
   Target: SQL Server (SSMS)
   Run on a clean database. Order of creation follows FK dependency:
   Users -> EventTypes -> Events -> Categories -> Enrolments -> Results
   ============================================================ */

-- ------------------------------------------------------------
-- Drop tables if they already exist (allows re-running cleanly)
-- Order matters: children before parents
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL DROP TABLE dbo.Enrolments;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.EventTypes', 'U') IS NOT NULL DROP TABLE dbo.EventTypes;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- ------------------------------------------------------------
-- USERS
-- Holds both Organisers and Participants; Role distinguishes them.
-- ------------------------------------------------------------
CREATE TABLE dbo.Users (
    UserID      INT IDENTITY(1,1) PRIMARY KEY,
    FirstName   NVARCHAR(50)  NOT NULL,
    LastName    NVARCHAR(50)  NOT NULL,
    Email       NVARCHAR(100) NOT NULL UNIQUE,
    Password    NVARCHAR(255) NOT NULL,   -- store a hash, never plain text
    Role        NVARCHAR(20)  NOT NULL DEFAULT 'Participant'
                CONSTRAINT CK_Users_Role CHECK (Role IN ('Organiser', 'Participant'))
);
GO

-- ------------------------------------------------------------
-- EVENT TYPES
-- Lookup table, e.g. Marathon, Fun Run, Trail Run
-- ------------------------------------------------------------
CREATE TABLE dbo.EventTypes (
    EventTypeID INT IDENTITY(1,1) PRIMARY KEY,
    TypeName    NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- ------------------------------------------------------------
-- EVENTS
-- Created by an Organiser (Users), classified by EventType
-- ------------------------------------------------------------
CREATE TABLE dbo.Events (
    EventID      INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID  INT NOT NULL,
    EventTypeID  INT NOT NULL,
    EventName    NVARCHAR(100) NOT NULL,
    Description  NVARCHAR(MAX) NULL,
    EventDate    DATE NOT NULL,
    Location     NVARCHAR(150) NOT NULL,
    Distance     DECIMAL(6,2) NULL,   -- kilometres
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (OrganiserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Events_EventType FOREIGN KEY (EventTypeID) REFERENCES dbo.EventTypes(EventTypeID)
    -- Note: DB does not enforce that OrganiserID belongs to a user with Role = 'Organiser';
    -- that check is done at the application layer.
);
GO

-- ------------------------------------------------------------
-- CATEGORIES
-- Sub-divisions within an Event (e.g. "Under 18", "Elite")
-- ------------------------------------------------------------
CREATE TABLE dbo.Categories (
    CategoryID   INT IDENTITY(1,1) PRIMARY KEY,
    EventID      INT NOT NULL,
    CategoryName NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Categories_Event FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID)
);
GO

-- ------------------------------------------------------------
-- ENROLMENTS
-- A Participant (Users) enrolling in a Category within an Event
-- ------------------------------------------------------------
CREATE TABLE dbo.Enrolments (
    EnrolmentID    INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID  INT NOT NULL,
    EventID        INT NOT NULL,
    CategoryID     INT NOT NULL,
    EnrolmentDate  DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    Status         NVARCHAR(20) NOT NULL DEFAULT 'Confirmed'
                   CONSTRAINT CK_Enrolments_Status CHECK (Status IN ('Confirmed', 'Pending', 'Cancelled')),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Enrolments_Event FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID),
    CONSTRAINT FK_Enrolments_Category FOREIGN KEY (CategoryID) REFERENCES dbo.Categories(CategoryID)
    -- Note: DB does not enforce that ParticipantID belongs to a user with Role = 'Participant',
    -- or that CategoryID actually belongs to EventID; both checked at the application layer.
);
GO

-- ------------------------------------------------------------
-- RESULTS
-- One optional Result per Enrolment (1 : 0..1)
-- ------------------------------------------------------------
CREATE TABLE dbo.Results (
    ResultID    INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,   -- UNIQUE enforces the 1 : 0..1 cardinality
    FinishTime  TIME(0) NULL,
    Position    INT NULL,
    CONSTRAINT FK_Results_Enrolment FOREIGN KEY (EnrolmentID) REFERENCES dbo.Enrolments(EnrolmentID)
);
GO

/* ============================================================
   SEED DATA
   ============================================================ */

-- Users: 2 Organisers, 2 Participants (plus 1 extra participant for variety)
INSERT INTO dbo.Users (FirstName, LastName, Email, Password, Role) VALUES
('Sarah', 'Botha',    'sarah.botha@raceday.co.za',    'HASHED_PWD_1', 'Organiser'),
('Themba','Nkosi',    'themba.nkosi@raceday.co.za',   'HASHED_PWD_2', 'Organiser'),
('Liam',  'Pretorius','liam.pretorius@example.com',   'HASHED_PWD_3', 'Participant'),
('Aisha', 'Khan',     'aisha.khan@example.com',       'HASHED_PWD_4', 'Participant'),
('Noah',  'van Wyk',  'noah.vanwyk@example.com',      'HASHED_PWD_5', 'Participant');
GO

-- Event Types
INSERT INTO dbo.EventTypes (TypeName) VALUES
('Marathon'),
('Fun Run'),
('Trail Run');
GO

-- Events: 3 events, one per organiser/type mix
INSERT INTO dbo.Events (OrganiserID, EventTypeID, EventName, Description, EventDate, Location, Distance) VALUES
(1, 1, 'Pretoria City Marathon',   'Annual road marathon through the city centre.', '2026-11-08', 'Pretoria, Gauteng', 42.20),
(1, 2, 'Family Fun Run',           'Casual 5km run for all ages.',                  '2026-10-04', 'Centurion, Gauteng', 5.00),
(2, 3, 'Magaliesberg Trail Run',   'Off-road trail run through the Magaliesberg.',  '2026-09-27', 'Magaliesberg, North West', 21.10);
GO

-- Categories: at least one per event
INSERT INTO dbo.Categories (EventID, CategoryName) VALUES
(1, 'Open'),
(1, 'Under 18'),
(2, 'Family (All Ages)'),
(3, 'Elite'),
(3, 'Novice');
GO

-- Enrolments: sample participants enrolling in categories
INSERT INTO dbo.Enrolments (ParticipantID, EventID, CategoryID, EnrolmentDate, Status) VALUES
(3, 1, 1, '2026-09-01', 'Confirmed'),
(4, 1, 2, '2026-09-02', 'Confirmed'),
(5, 2, 3, '2026-09-05', 'Confirmed'),
(3, 3, 4, '2026-09-10', 'Pending');
GO

-- Results: sample results for the enrolments that have already raced
INSERT INTO dbo.Results (EnrolmentID, FinishTime, Position) VALUES
(1, '03:45:12', 15),
(2, '04:02:30', 3);
GO
