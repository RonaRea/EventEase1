IF OBJECT_ID(N'dbo.Booking', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Booking
    (
        BookingId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EventId INT NOT NULL,
        VenueId INT NOT NULL,
        BookingDate DATETIME2 NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Event', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Event
    (
        EventId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EventName NVARCHAR(120) NOT NULL,
        EventDate DATE NOT NULL,
        EndDate DATE NOT NULL,
        Description NVARCHAR(1000) NOT NULL,
        ImageUrl NVARCHAR(500) NOT NULL,
        EventTypeId INT NOT NULL CONSTRAINT DF_Event_EventTypeId DEFAULT (1),
        VenueId INT NULL,
        CONSTRAINT CK_Event_DateRange CHECK (EndDate >= EventDate)
    );
END;
GO

IF OBJECT_ID(N'dbo.EventType', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventType
    (
        EventTypeId INT NOT NULL PRIMARY KEY,
        Name NVARCHAR(60) NOT NULL,
        SortOrder INT NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Venue', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Venue
    (
        VenueId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        VenueName NVARCHAR(100) NOT NULL,
        Location NVARCHAR(120) NOT NULL,
        Capacity INT NOT NULL,
        ImageUrl NVARCHAR(500) NOT NULL,
        IsAvailable BIT NOT NULL CONSTRAINT DF_Venue_IsAvailable DEFAULT (1)
    );
END;
GO

IF COL_LENGTH(N'dbo.Venue', N'IsAvailable') IS NULL
BEGIN
    ALTER TABLE dbo.Venue
        ADD IsAvailable BIT NOT NULL CONSTRAINT DF_Venue_IsAvailable DEFAULT (1);
END;
GO

IF COL_LENGTH(N'dbo.Event', N'EventTypeId') IS NULL
BEGIN
    ALTER TABLE dbo.Event
        ADD EventTypeId INT NOT NULL CONSTRAINT DF_Event_EventTypeId DEFAULT (1);
END;
GO

MERGE dbo.EventType AS target
USING (VALUES
    (1, N'Conference', 1),
    (2, N'Concert', 2),
    (3, N'Corporate', 3),
    (4, N'Expo', 4),
    (5, N'Workshop', 5)
) AS source (EventTypeId, Name, SortOrder)
ON target.EventTypeId = source.EventTypeId
WHEN MATCHED THEN
    UPDATE SET Name = source.Name, SortOrder = source.SortOrder
WHEN NOT MATCHED BY TARGET THEN
    INSERT (EventTypeId, Name, SortOrder)
    VALUES (source.EventTypeId, source.Name, source.SortOrder);
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Booking_EventId'
      AND object_id = OBJECT_ID(N'dbo.Booking'))
BEGIN
    CREATE UNIQUE INDEX IX_Booking_EventId ON dbo.Booking (EventId);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Booking_VenueId'
      AND object_id = OBJECT_ID(N'dbo.Booking'))
BEGIN
    CREATE INDEX IX_Booking_VenueId ON dbo.Booking (VenueId);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Event_VenueId'
      AND object_id = OBJECT_ID(N'dbo.Event'))
BEGIN
    CREATE INDEX IX_Event_VenueId ON dbo.Event (VenueId);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Event_EventTypeId'
      AND object_id = OBJECT_ID(N'dbo.Event'))
BEGIN
    CREATE INDEX IX_Event_EventTypeId ON dbo.Event (EventTypeId);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Event_Venue_VenueId')
BEGIN
    ALTER TABLE dbo.Event
        ADD CONSTRAINT FK_Event_Venue_VenueId
        FOREIGN KEY (VenueId) REFERENCES dbo.Venue (VenueId)
        ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Event_EventType_EventTypeId')
BEGIN
    ALTER TABLE dbo.Event
        ADD CONSTRAINT FK_Event_EventType_EventTypeId
        FOREIGN KEY (EventTypeId) REFERENCES dbo.EventType (EventTypeId);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Booking_Event_EventId')
BEGIN
    ALTER TABLE dbo.Booking
        ADD CONSTRAINT FK_Booking_Event_EventId
        FOREIGN KEY (EventId) REFERENCES dbo.Event (EventId);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Booking_Venue_VenueId')
BEGIN
    ALTER TABLE dbo.Booking
        ADD CONSTRAINT FK_Booking_Venue_VenueId
        FOREIGN KEY (VenueId) REFERENCES dbo.Venue (VenueId);
END;
GO

CREATE OR ALTER VIEW dbo.vwBookingOverview AS
SELECT
    b.BookingId,
    b.BookingDate,
    e.EventId,
    e.EventName,
    e.EventDate,
    e.EndDate,
    e.Description AS EventDescription,
    e.ImageUrl AS EventImageUrl,
    v.VenueId,
    v.VenueName,
    v.Location AS VenueLocation,
    v.Capacity AS VenueCapacity,
    v.ImageUrl AS VenueImageUrl
FROM dbo.Booking AS b
INNER JOIN dbo.Event AS e ON b.EventId = e.EventId
INNER JOIN dbo.Venue AS v ON b.VenueId = v.VenueId;
GO
