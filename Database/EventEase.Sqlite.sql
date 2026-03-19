PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Venue (
    VenueId INTEGER PRIMARY KEY AUTOINCREMENT,
    VenueName TEXT NOT NULL,
    Location TEXT NOT NULL,
    Capacity INTEGER NOT NULL,
    ImageUrl TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Event (
    EventId INTEGER PRIMARY KEY AUTOINCREMENT,
    EventName TEXT NOT NULL,
    EventDate TEXT NOT NULL,
    EndDate TEXT NOT NULL,
    Description TEXT NOT NULL,
    VenueId INTEGER NULL,
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
CREATE INDEX IF NOT EXISTS IX_Event_VenueId ON Event (VenueId);
