using EventEase.Web.Data;
using EventEase.Web.Models;
using EventEase.Web.Models.ViewModels;
using EventEase.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Web.Controllers;

public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IImageStorageService _imageStorageService;

    public EventsController(ApplicationDbContext context, IImageStorageService imageStorageService)
    {
        _context = context;
        _imageStorageService = imageStorageService;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? eventTypeId, DateTime? fromDate, DateTime? toDate, string? status, string? venueAvailability)
    {
        ViewData["SearchTerm"] = searchTerm;
        ViewData["EventTypeId"] = eventTypeId;
        ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
        ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");
        ViewData["Status"] = status;
        ViewData["VenueAvailability"] = venueAvailability;

        await PopulateEventTypeFilterAsync(eventTypeId);

        var eventsQuery = _context.Events
            .AsNoTracking()
            .Include(eventItem => eventItem.EventType)
            .Include(eventItem => eventItem.Venue)
            .Include(eventItem => eventItem.Bookings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var likePattern = $"%{searchTerm.Trim()}%";
            eventsQuery = eventsQuery.Where(eventItem =>
                EF.Functions.Like(eventItem.EventName, likePattern) ||
                EF.Functions.Like(eventItem.Description, likePattern) ||
                EF.Functions.Like(eventItem.EventType!.Name, likePattern));
        }

        if (eventTypeId.HasValue)
        {
            eventsQuery = eventsQuery.Where(eventItem => eventItem.EventTypeId == eventTypeId.Value);
        }

        if (fromDate.HasValue)
        {
            eventsQuery = eventsQuery.Where(eventItem => eventItem.EndDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            eventsQuery = eventsQuery.Where(eventItem => eventItem.EventDate <= toDate.Value.Date);
        }

        if (string.Equals(status, "booked", StringComparison.OrdinalIgnoreCase))
        {
            eventsQuery = eventsQuery.Where(eventItem => eventItem.VenueId != null);
        }
        else if (string.Equals(status, "unbooked", StringComparison.OrdinalIgnoreCase))
        {
            eventsQuery = eventsQuery.Where(eventItem => eventItem.VenueId == null);
        }

        if (string.Equals(venueAvailability, "available", StringComparison.OrdinalIgnoreCase))
        {
            eventsQuery = eventsQuery.Where(eventItem => eventItem.VenueId != null && eventItem.Venue != null && eventItem.Venue.IsAvailable);
        }
        else if (string.Equals(venueAvailability, "unavailable", StringComparison.OrdinalIgnoreCase))
        {
            eventsQuery = eventsQuery.Where(eventItem => eventItem.VenueId != null && eventItem.Venue != null && !eventItem.Venue.IsAvailable);
        }
        else if (string.Equals(venueAvailability, "unassigned", StringComparison.OrdinalIgnoreCase))
        {
            eventsQuery = eventsQuery.Where(eventItem => eventItem.VenueId == null);
        }

        var events = await eventsQuery
            .OrderBy(eventItem => eventItem.EventDate)
            .ThenBy(eventItem => eventItem.EventName)
            .ToListAsync();

        return View(events);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var eventItem = await _context.Events
            .AsNoTracking()
            .Include(item => item.EventType)
            .Include(item => item.Venue)
            .Include(item => item.Bookings)
                .ThenInclude(booking => booking.Venue)
            .FirstOrDefaultAsync(item => item.EventId == id.Value);

        if (eventItem == null)
        {
            return NotFound();
        }

        return View(eventItem);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateEventTypeOptionsAsync();

        return View(new EventFormViewModel
        {
            EventTypeId = await GetDefaultEventTypeIdAsync(),
            EventDate = DateTime.Today,
            EndDate = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        model.EventDate = model.EventDate.Date;
        model.EndDate = model.EndDate.Date;

        if (model.ImageFile == null)
        {
            ModelState.AddModelError(nameof(model.ImageFile), "Please upload an event image.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateEventTypeOptionsAsync(model.EventTypeId);
            return View(model);
        }

        string imageUrl;

        try
        {
            imageUrl = await _imageStorageService.UploadEventImageAsync(model.ImageFile!);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
            await PopulateEventTypeOptionsAsync(model.EventTypeId);
            return View(model);
        }

        var eventItem = new Event
        {
            EventName = model.EventName,
            EventTypeId = model.EventTypeId,
            EventDate = model.EventDate,
            EndDate = model.EndDate,
            Description = model.Description,
            ImageUrl = imageUrl
        };

        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Event created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var eventItem = await _context.Events.FindAsync(id.Value);
        if (eventItem == null)
        {
            return NotFound();
        }

        await PopulateEventTypeOptionsAsync(eventItem.EventTypeId);

        return View(new EventFormViewModel
        {
            EventId = eventItem.EventId,
            EventName = eventItem.EventName,
            EventTypeId = eventItem.EventTypeId,
            EventDate = eventItem.EventDate,
            EndDate = eventItem.EndDate,
            Description = eventItem.Description,
            ExistingImageUrl = eventItem.ImageUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        if (id != model.EventId)
        {
            return NotFound();
        }

        model.EventDate = model.EventDate.Date;
        model.EndDate = model.EndDate.Date;

        var existingEvent = await _context.Events.FindAsync(id);
        if (existingEvent == null)
        {
            return NotFound();
        }

        model.ExistingImageUrl = existingEvent.ImageUrl;

        var existingBooking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(booking => booking.EventId == id);

        if (existingBooking != null)
        {
            var hasConflict = await HasVenueConflictAsync(
                existingBooking.VenueId,
                model.EventDate,
                model.EndDate,
                existingBooking.BookingId);

            if (hasConflict)
            {
                ModelState.AddModelError(string.Empty, "Changing this event's date range would create a double booking for the assigned venue.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateEventTypeOptionsAsync(model.EventTypeId);
            return View(model);
        }

        if (model.ImageFile != null)
        {
            try
            {
                existingEvent.ImageUrl = await _imageStorageService.UploadEventImageAsync(model.ImageFile);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                await PopulateEventTypeOptionsAsync(model.EventTypeId);
                return View(model);
            }
        }

        existingEvent.EventName = model.EventName;
        existingEvent.EventTypeId = model.EventTypeId;
        existingEvent.EventDate = model.EventDate;
        existingEvent.EndDate = model.EndDate;
        existingEvent.Description = model.Description;

        if (existingBooking == null)
        {
            existingEvent.VenueId = null;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Event updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var eventItem = await _context.Events
            .AsNoTracking()
            .Include(item => item.Bookings)
            .FirstOrDefaultAsync(item => item.EventId == id.Value);

        if (eventItem == null)
        {
            return NotFound();
        }

        ViewData["DeleteBlocked"] = eventItem.Bookings.Any();
        return View(eventItem);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var eventItem = await _context.Events
            .Include(existingEvent => existingEvent.Bookings)
            .FirstOrDefaultAsync(existingEvent => existingEvent.EventId == id);

        if (eventItem == null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (eventItem.Bookings.Any())
        {
            TempData["ErrorMessage"] = "This event cannot be deleted because it is linked to an existing booking.";
            return RedirectToAction(nameof(Index));
        }

        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Event deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> HasVenueConflictAsync(int venueId, DateTime startDate, DateTime endDate, int? ignoreBookingId = null)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Event)
            .AnyAsync(booking =>
                booking.VenueId == venueId &&
                booking.BookingId != ignoreBookingId &&
                booking.Event != null &&
                booking.Event.EventDate <= endDate &&
                booking.Event.EndDate >= startDate);
    }

    private async Task PopulateEventTypeOptionsAsync(int? selectedEventTypeId = null)
    {
        var eventTypes = await _context.EventTypes
            .AsNoTracking()
            .OrderBy(eventType => eventType.SortOrder)
            .ThenBy(eventType => eventType.Name)
            .ToListAsync();

        ViewBag.EventTypeId = new SelectList(eventTypes, "EventTypeId", "Name", selectedEventTypeId);
    }

    private async Task PopulateEventTypeFilterAsync(int? selectedEventTypeId)
    {
        var eventTypes = await _context.EventTypes
            .AsNoTracking()
            .OrderBy(eventType => eventType.SortOrder)
            .ThenBy(eventType => eventType.Name)
            .ToListAsync();

        ViewBag.EventTypeFilter = new SelectList(eventTypes, "EventTypeId", "Name", selectedEventTypeId);
    }

    private async Task<int> GetDefaultEventTypeIdAsync()
    {
        return await _context.EventTypes
            .AsNoTracking()
            .OrderBy(eventType => eventType.SortOrder)
            .Select(eventType => eventType.EventTypeId)
            .FirstAsync();
    }
}
