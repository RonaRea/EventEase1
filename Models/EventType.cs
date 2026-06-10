using System.ComponentModel.DataAnnotations;

namespace EventEase.Web.Models;

public class EventType
{
    public int EventTypeId { get; set; }

    [Required]
    [StringLength(60)]
    [Display(Name = "Event type")]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public ICollection<Event> Events { get; set; } = [];
}