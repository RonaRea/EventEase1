using System.ComponentModel.DataAnnotations;

namespace EventEase.Web.Models;

public class Booking
{
    public int BookingId { get; set; }

    [Display(Name = "Event")]
    public int EventId { get; set; }

    [Display(Name = "Venue")]
    public int VenueId { get; set; }

    [Display(Name = "Booked on")]
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    public Event? Event { get; set; }
    public Venue? Venue { get; set; }
}
