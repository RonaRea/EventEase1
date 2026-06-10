using EventEase.Web.Data;
using EventEase.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Web.Controllers;

public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? venueId, DateTime? fromDate, DateTime? toDate)
    {
        ViewData["SearchTerm"] = searchTerm;
        ViewData["VenueId"] = venueId;
        ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
        ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

        await PopulateVenueFilterAsync(venueId);

        var bookingsQuery = _context.BookingOverview
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var trimmedSearch = searchTerm.Trim();
            var likePattern = $"%{trimmedSearch}%";
            var hasBookingId = int.TryParse(trimmedSearch, out var bookingId);

            bookingsQuery = bookingsQuery.Where(booking =>
                EF.Functions.Like(booking.EventName, likePattern) ||
                (hasBookingId && booking.BookingId == bookingId));
        }

        if (venueId.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(booking => booking.VenueId == venueId.Value);
        }

        if (fromDate.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(booking => booking.EndDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(booking => booking.EventDate <= toDate.Value.Date);
        }

        var bookings = await bookingsQuery
            .OrderBy(booking => booking.EventDate)
            .ThenBy(booking => booking.EventName)
            .ToListAsync();

        return View(bookings);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(item => item.Event)
            .Include(item => item.Venue)
            .FirstOrDefaultAsync(item => item.BookingId == id.Value);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateSelectionsAsync();

        return View(new Booking
        {
            BookingDate = DateTime.UtcNow
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EventId,VenueId")] Booking booking)
    {
        var eventItem = await _context.Events.FirstOrDefaultAsync(item => item.EventId == booking.EventId);
        if (eventItem == null)
        {
            ModelState.AddModelError(nameof(booking.EventId), "Please select a valid event.");
        }

        var eventAlreadyBooked = await _context.Bookings.AnyAsync(existingBooking => existingBooking.EventId == booking.EventId);
        if (eventAlreadyBooked)
        {
            ModelState.AddModelError(nameof(booking.EventId), "The selected event already has a booking.");
        }

        var selectedVenue = await _context.Venues.FirstOrDefaultAsync(item => item.VenueId == booking.VenueId);
        if (selectedVenue == null)
        {
            ModelState.AddModelError(nameof(booking.VenueId), "Please select a valid venue.");
        }
        else if (!selectedVenue.IsAvailable)
        {
            ModelState.AddModelError(nameof(booking.VenueId), "The selected venue is marked as unavailable for new bookings.");
        }

        if (eventItem != null)
        {
            var hasConflict = await HasVenueConflictAsync(booking.VenueId, eventItem.EventDate, eventItem.EndDate);
            if (hasConflict)
            {
                ModelState.AddModelError(nameof(booking.VenueId), "The selected venue is not available for the chosen event dates.");
            }
        }

        if (!ModelState.IsValid)
        {
            booking.BookingDate = DateTime.UtcNow;
            await PopulateSelectionsAsync(booking.EventId, booking.VenueId);
            return View(booking);
        }

        var newBooking = new Booking
        {
            EventId = booking.EventId,
            VenueId = booking.VenueId,
            BookingDate = DateTime.UtcNow
        };

        _context.Bookings.Add(newBooking);
        eventItem!.VenueId = booking.VenueId;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Booking created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(item => item.Event)
            .Include(item => item.Venue)
            .FirstOrDefaultAsync(item => item.BookingId == id.Value);

        if (booking == null)
        {
            return NotFound();
        }

        await PopulateSelectionsAsync(booking.EventId, booking.VenueId, booking.BookingId);
        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId")] Booking booking)
    {
        if (id != booking.BookingId)
        {
            return NotFound();
        }

        var existingBooking = await _context.Bookings
            .Include(item => item.Event)
            .FirstOrDefaultAsync(item => item.BookingId == id);

        if (existingBooking == null)
        {
            return NotFound();
        }

        var selectedEvent = await _context.Events.FirstOrDefaultAsync(item => item.EventId == booking.EventId);
        if (selectedEvent == null)
        {
            ModelState.AddModelError(nameof(booking.EventId), "Please select a valid event.");
        }

        var eventAlreadyBooked = await _context.Bookings.AnyAsync(item => item.EventId == booking.EventId && item.BookingId != id);
        if (eventAlreadyBooked)
        {
            ModelState.AddModelError(nameof(booking.EventId), "The selected event already has a booking.");
        }

        var selectedVenue = await _context.Venues.FirstOrDefaultAsync(item => item.VenueId == booking.VenueId);
        if (selectedVenue == null)
        {
            ModelState.AddModelError(nameof(booking.VenueId), "Please select a valid venue.");
        }
        else if (!selectedVenue.IsAvailable && selectedVenue.VenueId != existingBooking.VenueId)
        {
            ModelState.AddModelError(nameof(booking.VenueId), "The selected venue is marked as unavailable for new bookings.");
        }

        if (selectedEvent != null)
        {
            var hasConflict = await HasVenueConflictAsync(booking.VenueId, selectedEvent.EventDate, selectedEvent.EndDate, booking.BookingId);
            if (hasConflict)
            {
                ModelState.AddModelError(nameof(booking.VenueId), "The selected venue is not available for the chosen event dates.");
            }
        }

        if (!ModelState.IsValid)
        {
            booking.BookingDate = existingBooking.BookingDate;
            await PopulateSelectionsAsync(booking.EventId, booking.VenueId, booking.BookingId);
            return View(booking);
        }

        var previousEventId = existingBooking.EventId;
        Event? previousEvent = null;

        if (previousEventId != booking.EventId)
        {
            previousEvent = await _context.Events.FirstOrDefaultAsync(item => item.EventId == previousEventId);
        }

        existingBooking.EventId = booking.EventId;
        existingBooking.VenueId = booking.VenueId;

        if (previousEvent != null)
        {
            previousEvent.VenueId = null;
        }

        selectedEvent!.VenueId = booking.VenueId;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Booking updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(item => item.Event)
            .Include(item => item.Venue)
            .FirstOrDefaultAsync(item => item.BookingId == id.Value);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(item => item.BookingId == id);
        if (booking == null)
        {
            return RedirectToAction(nameof(Index));
        }

        var eventItem = await _context.Events.FirstOrDefaultAsync(item => item.EventId == booking.EventId);
        if (eventItem != null)
        {
            eventItem.VenueId = null;
        }

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Booking deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateSelectionsAsync(int? selectedEventId = null, int? selectedVenueId = null, int? currentBookingId = null)
    {
        var reservedEventIdsQuery = _context.Bookings.AsNoTracking();

        if (currentBookingId.HasValue)
        {
            reservedEventIdsQuery = reservedEventIdsQuery.Where(booking => booking.BookingId != currentBookingId.Value);
        }

        var reservedEventIds = await reservedEventIdsQuery
            .Select(booking => booking.EventId)
            .ToListAsync();

        var eventOptions = await _context.Events
            .AsNoTracking()
            .Where(eventItem => !reservedEventIds.Contains(eventItem.EventId) || eventItem.EventId == selectedEventId)
            .OrderBy(eventItem => eventItem.EventDate)
            .ThenBy(eventItem => eventItem.EventName)
            .Select(eventItem => new
            {
                eventItem.EventId,
                Label = eventItem.EventName + " (" + eventItem.EventDate.ToString("dd MMM yyyy") + " to " + eventItem.EndDate.ToString("dd MMM yyyy") + ")"
            })
            .ToListAsync();

        var venueOptions = await _context.Venues
            .AsNoTracking()
            .Where(venue => venue.IsAvailable || venue.VenueId == selectedVenueId)
            .OrderBy(venue => venue.VenueName)
            .Select(venue => new
            {
                venue.VenueId,
                Label = venue.VenueName + " (" + venue.Location + ")" + (venue.IsAvailable ? string.Empty : " - unavailable")
            })
            .ToListAsync();

        ViewBag.HasAvailableEvents = eventOptions.Count > 0;
        ViewBag.EventId = new SelectList(eventOptions, "EventId", "Label", selectedEventId);
        ViewBag.VenueId = new SelectList(venueOptions, "VenueId", "Label", selectedVenueId);
    }

    private async Task PopulateVenueFilterAsync(int? selectedVenueId)
    {
        var venues = await _context.Venues
            .AsNoTracking()
            .OrderBy(venue => venue.VenueName)
            .ToListAsync();

        ViewBag.VenueFilter = new SelectList(venues, "VenueId", "VenueName", selectedVenueId);
    }

    private async Task<bool> HasVenueConflictAsync(int venueId, DateTime startDate, DateTime endDate, int? ignoreBookingId = null)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(existingBooking => existingBooking.Event)
            .AnyAsync(existingBooking =>
                existingBooking.VenueId == venueId &&
                existingBooking.BookingId != ignoreBookingId &&
                existingBooking.Event != null &&
                existingBooking.Event.EventDate <= endDate &&
                existingBooking.Event.EndDate >= startDate);
    }
}
