using System.ComponentModel.DataAnnotations;

namespace EventEase.Web.Models;

public class Event : IValidatableObject
{
    public int EventId { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Event name")]
    public string EventName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime EventDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime EndDate { get; set; } = DateTime.Today;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Assigned venue")]
    public int? VenueId { get; set; }

    public Venue? Venue { get; set; }
    public ICollection<Booking> Bookings { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate.Date < EventDate.Date)
        {
            yield return new ValidationResult(
                "End date must be on or after the start date.",
                [nameof(EndDate)]);
        }
    }
}
