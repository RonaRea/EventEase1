using EventEase.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingOverview> BookingOverview => Set<BookingOverview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.ToTable("Venue");
            entity.HasKey(venue => venue.VenueId);
            entity.Property(venue => venue.VenueName).HasMaxLength(100).IsRequired();
            entity.Property(venue => venue.Location).HasMaxLength(120).IsRequired();
            entity.Property(venue => venue.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(venue => venue.IsAvailable).HasDefaultValue(true);
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.ToTable("EventType");
            entity.HasKey(eventType => eventType.EventTypeId);
            entity.Property(eventType => eventType.Name).HasMaxLength(60).IsRequired();

            entity.HasData(
                new EventType { EventTypeId = 1, Name = "Conference", SortOrder = 1 },
                new EventType { EventTypeId = 2, Name = "Concert", SortOrder = 2 },
                new EventType { EventTypeId = 3, Name = "Corporate", SortOrder = 3 },
                new EventType { EventTypeId = 4, Name = "Expo", SortOrder = 4 },
                new EventType { EventTypeId = 5, Name = "Workshop", SortOrder = 5 });
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Event");
            entity.HasKey(eventItem => eventItem.EventId);
            entity.Property(eventItem => eventItem.EventName).HasMaxLength(120).IsRequired();
            entity.Property(eventItem => eventItem.Description).HasMaxLength(1000).IsRequired();
            entity.Property(eventItem => eventItem.ImageUrl).HasMaxLength(500).IsRequired();

            entity.HasOne(eventItem => eventItem.EventType)
                .WithMany(eventType => eventType.Events)
                .HasForeignKey(eventItem => eventItem.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(eventItem => eventItem.Venue)
                .WithMany(venue => venue.Events)
                .HasForeignKey(eventItem => eventItem.VenueId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Booking");
            entity.HasKey(booking => booking.BookingId);
            entity.HasIndex(booking => booking.EventId).IsUnique();

            entity.HasOne(booking => booking.Event)
                .WithMany(eventItem => eventItem.Bookings)
                .HasForeignKey(booking => booking.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(booking => booking.Venue)
                .WithMany(venue => venue.Bookings)
                .HasForeignKey(booking => booking.VenueId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookingOverview>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vwBookingOverview");
            entity.Property(item => item.EventName).HasMaxLength(120);
            entity.Property(item => item.EventDescription).HasMaxLength(1000);
            entity.Property(item => item.EventImageUrl).HasMaxLength(500);
            entity.Property(item => item.VenueName).HasMaxLength(100);
            entity.Property(item => item.VenueLocation).HasMaxLength(120);
            entity.Property(item => item.VenueImageUrl).HasMaxLength(500);
        });
    }
}
