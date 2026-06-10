PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS EventType (
    EventTypeId INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    SortOrder INTEGER NOT NULL
);

INSERT OR IGNORE INTO EventType (EventTypeId, Name, SortOrder) VALUES
    (1, 'Conference', 1),
    (2, 'Concert', 2),
    (3, 'Corporate', 3),
    (4, 'Expo', 4),
    (5, 'Workshop', 5);

CREATE TABLE IF NOT EXISTS Venue (
    VenueId INTEGER PRIMARY KEY AUTOINCREMENT,
    VenueName TEXT NOT NULL,
    Location TEXT NOT NULL,
    Capacity INTEGER NOT NULL,
    ImageUrl TEXT NOT NULL,
    IsAvailable INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Event (
    EventId INTEGER PRIMARY KEY AUTOINCREMENT,
    EventName TEXT NOT NULL,
    EventDate TEXT NOT NULL,
    EndDate TEXT NOT NULL,
    Description TEXT NOT NULL,
    ImageUrl TEXT NOT NULL,
    EventTypeId INTEGER NOT NULL DEFAULT 1,
    VenueId INTEGER NULL,
    CONSTRAINT FK_Event_EventType_EventTypeId
        FOREIGN KEY (EventTypeId) REFERENCES EventType (EventTypeId)
        ON DELETE RESTRICT,
    CONSTRAINT FK_Event_Venue_VenueId
        FOREIGN KEY (VenueId) REFERENCES Venue (VenueId)
        ON DELETE SET NULL,
    CONSTRAINT CK_Event_DateRange
        CHECK (date(EndDate) >= date(EventDate))
);

CREATE TABLE IF NOT EXISTS Booking (
    BookingId INTEGER PRIMARY KEY AUTOINCREMENT,
    EventId INTEGER NOT NULL,
    VenueId INTEGER NOT NULL,
    BookingDate TEXT NOT NULL,
    CONSTRAINT FK_Booking_Event_EventId
        FOREIGN KEY (EventId) REFERENCES Event (EventId)
        ON DELETE RESTRICT,
    CONSTRAINT FK_Booking_Venue_VenueId
        FOREIGN KEY (VenueId) REFERENCES Venue (VenueId)
        ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Booking_EventId ON Booking (EventId);
CREATE INDEX IF NOT EXISTS IX_Booking_VenueId ON Booking (VenueId);
CREATE INDEX IF NOT EXISTS IX_Event_EventTypeId ON Event (EventTypeId);
CREATE INDEX IF NOT EXISTS IX_Event_VenueId ON Event (VenueId);

DROP VIEW IF EXISTS vwBookingOverview;

CREATE VIEW vwBookingOverview AS
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
FROM Booking AS b
INNER JOIN Event AS e ON b.EventId = e.EventId
INNER JOIN Venue AS v ON b.VenueId = v.VenueId;
