using EventEase.Web.Data;
using EventEase.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Web.Controllers;

public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;

    public EventsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm, DateTime? fromDate, DateTime? toDate, string? status)
    {
        ViewData["SearchTerm"] = searchTerm;
        ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
        ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");
        ViewData["Status"] = status;

        var eventsQuery = _context.Events
            .AsNoTracking()
            .Include(eventItem => eventItem.Venue)
            .Include(eventItem => eventItem.Bookings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var likePattern = $"%{searchTerm.Trim()}%";
            eventsQuery = eventsQuery.Where(eventItem =>
                EF.Functions.Like(eventItem.EventName, likePattern) ||
                EF.Functions.Like(eventItem.Description, likePattern));
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

    public IActionResult Create()
    {
        return View(new Event
        {
            EventDate = DateTime.Today,
            EndDate = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EventName,EventDate,EndDate,Description")] Event eventItem)
    {
        eventItem.EventDate = eventItem.EventDate.Date;
        eventItem.EndDate = eventItem.EndDate.Date;

        if (!ModelState.IsValid)
        {
            return View(eventItem);
        }

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

        return View(eventItem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,EndDate,Description")] Event eventItem)
    {
        if (id != eventItem.EventId)
        {
            return NotFound();
        }

        eventItem.EventDate = eventItem.EventDate.Date;
        eventItem.EndDate = eventItem.EndDate.Date;

        var existingEvent = await _context.Events.FindAsync(id);
        if (existingEvent == null)
        {
            return NotFound();
        }

        var existingBooking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(booking => booking.EventId == id);

        if (existingBooking != null)
        {
            var hasConflict = await HasVenueConflictAsync(
                existingBooking.VenueId,
                eventItem.EventDate,
                eventItem.EndDate,
                existingBooking.BookingId);

            if (hasConflict)
            {
                ModelState.AddModelError(string.Empty, "Changing this event's date range would create a double booking for the assigned venue.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(eventItem);
        }

        existingEvent.EventName = eventItem.EventName;
        existingEvent.EventDate = eventItem.EventDate;
        existingEvent.EndDate = eventItem.EndDate;
        existingEvent.Description = eventItem.Description;

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
}
