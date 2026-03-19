using System.ComponentModel.DataAnnotations;

namespace EventEase.Web.Models;

public class Venue
{
    public int VenueId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Venue name")]
    public string VenueName { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Location { get; set; } = string.Empty;

    [Range(1, 500000)]
    public int Capacity { get; set; }

    [Required]
    [StringLength(500)]
    [Url]
    [Display(Name = "Image URL")]
    public string ImageUrl { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
}
