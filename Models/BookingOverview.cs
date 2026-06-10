namespace EventEase.Web.Models;

public class BookingOverview
{
    public int BookingId { get; set; }
    public DateTime BookingDate { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime EndDate { get; set; }
    public string EventDescription { get; set; } = string.Empty;
    public string EventImageUrl { get; set; } = string.Empty;
    public int VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string VenueLocation { get; set; } = string.Empty;
    public int VenueCapacity { get; set; }
    public string VenueImageUrl { get; set; } = string.Empty;
}
