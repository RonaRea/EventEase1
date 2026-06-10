using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EventEase.Web.Models.ViewModels;

public class EventFormViewModel : IValidatableObject
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

    [Range(1, int.MaxValue, ErrorMessage = "Please select an event type.")]
    [Display(Name = "Event type")]
    public int EventTypeId { get; set; }

    [Display(Name = "Event image")]
    public IFormFile? ImageFile { get; set; }

    public string ExistingImageUrl { get; set; } = string.Empty;

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
