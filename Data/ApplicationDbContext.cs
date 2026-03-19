using EventEase.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Booking> Bookings => Set<Booking>();

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
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Event");
            entity.HasKey(eventItem => eventItem.EventId);
            entity.Property(eventItem => eventItem.EventName).HasMaxLength(120).IsRequired();
            entity.Property(eventItem => eventItem.Description).HasMaxLength(1000).IsRequired();

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
    }
}
