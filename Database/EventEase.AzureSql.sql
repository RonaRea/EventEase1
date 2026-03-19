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
        VenueId INT NULL,
        CONSTRAINT CK_Event_DateRange CHECK (EndDate >= EventDate)
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
        ImageUrl NVARCHAR(500) NOT NULL
    );
END;
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
