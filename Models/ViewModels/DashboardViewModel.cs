namespace EventEase.Web.Models.ViewModels;

public class DashboardViewModel
{
    public int VenueCount { get; init; }
    public int EventCount { get; init; }
    public int BookingCount { get; init; }
    public int PendingEventCount { get; init; }
    public int AvailableVenueCountToday { get; init; }
    public IReadOnlyList<Event> UpcomingEvents { get; init; } = [];
    public IReadOnlyList<Booking> RecentBookings { get; init; } = [];
    public IReadOnlyList<Venue> AvailableVenuesToday { get; init; } = [];
}
