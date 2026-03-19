using EventEase.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Venues.AnyAsync())
        {
            return;
        }

        var venues = new List<Venue>
        {
            new()
            {
                VenueName = "Harbour Lights Hall",
                Location = "Cape Town Waterfront",
                Capacity = 320,
                ImageUrl = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?auto=format&fit=crop&w=900&q=80"
            },
            new()
            {
                VenueName = "Summit Conference Centre",
                Location = "Sandton, Johannesburg",
                Capacity = 500,
                ImageUrl = "https://images.unsplash.com/photo-1497366754035-f200968a6e72?auto=format&fit=crop&w=900&q=80"
            },
            new()
            {
                VenueName = "Garden Grove Pavilion",
                Location = "Durban North",
                Capacity = 180,
                ImageUrl = "https://images.unsplash.com/photo-1511795409834-ef04bbd61622?auto=format&fit=crop&w=900&q=80"
            },
            new()
            {
                VenueName = "Skyline Terrace",
                Location = "Pretoria Central",
                Capacity = 140,
                ImageUrl = "https://images.unsplash.com/photo-1505236858219-8359eb29e329?auto=format&fit=crop&w=900&q=80"
            }
        };

        await context.Venues.AddRangeAsync(venues);
        await context.SaveChangesAsync();

        var events = new List<Event>
        {
            new()
            {
                EventName = "Startup South Summit",
                EventDate = DateTime.Today.AddDays(10),
                EndDate = DateTime.Today.AddDays(12),
                Description = "A three-day conference for founders, investors, and technical teams."
            },
            new()
            {
                EventName = "Wellness Weekend Expo",
                EventDate = DateTime.Today.AddDays(18),
                EndDate = DateTime.Today.AddDays(18),
                Description = "Community-focused wellness sessions, screenings, and product showcases."
            },
            new()
            {
                EventName = "Midyear Jazz Evening",
                EventDate = DateTime.Today.AddDays(24),
                EndDate = DateTime.Today.AddDays(24),
                Description = "An intimate evening performance with local jazz artists and food vendors."
            },
            new()
            {
                EventName = "Design Futures Forum",
                EventDate = DateTime.Today.AddDays(33),
                EndDate = DateTime.Today.AddDays(34),
                Description = "A design and innovation forum awaiting final venue confirmation."
            }
        };

        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();

        var bookings = new List<Booking>
        {
            new()
            {
                EventId = events[0].EventId,
                VenueId = venues[1].VenueId,
                BookingDate = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                EventId = events[1].EventId,
                VenueId = venues[2].VenueId,
                BookingDate = DateTime.UtcNow.AddDays(-5)
            }
        };

        events[0].VenueId = venues[1].VenueId;
        events[1].VenueId = venues[2].VenueId;

        await context.Bookings.AddRangeAsync(bookings);
        await context.SaveChangesAsync();
    }
}
