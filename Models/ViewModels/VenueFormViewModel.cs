using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EventEase.Web.Models.ViewModels;

public class VenueFormViewModel
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

    [Display(Name = "Available for bookings")]
    public bool IsAvailable { get; set; } = true;

    [Display(Name = "Venue image")]
    public IFormFile? ImageFile { get; set; }

    public string ExistingImageUrl { get; set; } = string.Empty;
}
