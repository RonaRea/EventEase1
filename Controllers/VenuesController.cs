using EventEase.Web.Data;
using EventEase.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Web.Controllers;

public class VenuesController : Controller
{
    private readonly ApplicationDbContext _context;

    public VenuesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? minCapacity)
    {
        ViewData["SearchTerm"] = searchTerm;
        ViewData["MinCapacity"] = minCapacity;

        var venuesQuery = _context.Venues
            .AsNoTracking()
            .Include(venue => venue.Bookings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var likePattern = $"%{searchTerm.Trim()}%";
            venuesQuery = venuesQuery.Where(venue =>
                EF.Functions.Like(venue.VenueName, likePattern) ||
                EF.Functions.Like(venue.Location, likePattern));
        }

        if (minCapacity.HasValue)
        {
            venuesQuery = venuesQuery.Where(venue => venue.Capacity >= minCapacity.Value);
        }

        var venues = await venuesQuery
            .OrderBy(venue => venue.VenueName)
            .ToListAsync();

        return View(venues);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var venue = await _context.Venues
            .AsNoTracking()
            .Include(venue => venue.Events)
            .Include(venue => venue.Bookings)
                .ThenInclude(booking => booking.Event)
            .FirstOrDefaultAsync(venue => venue.VenueId == id.Value);

        if (venue == null)
        {
            return NotFound();
        }

        return View(venue);
    }

    public IActionResult Create()
    {
        return View(new Venue());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("VenueName,Location,Capacity,ImageUrl")] Venue venue)
    {
        if (!ModelState.IsValid)
        {
            return View(venue);
        }

        _context.Venues.Add(venue);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Venue created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var venue = await _context.Venues.FindAsync(id.Value);
        if (venue == null)
        {
            return NotFound();
        }

        return View(venue);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageUrl")] Venue venue)
    {
        if (id != venue.VenueId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(venue);
        }

        var existingVenue = await _context.Venues.FindAsync(id);
        if (existingVenue == null)
        {
            return NotFound();
        }

        existingVenue.VenueName = venue.VenueName;
        existingVenue.Location = venue.Location;
        existingVenue.Capacity = venue.Capacity;
        existingVenue.ImageUrl = venue.ImageUrl;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Venue updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var venue = await _context.Venues
            .AsNoTracking()
            .Include(venue => venue.Bookings)
            .FirstOrDefaultAsync(venue => venue.VenueId == id.Value);

        if (venue == null)
        {
            return NotFound();
        }

        ViewData["DeleteBlocked"] = venue.Bookings.Any();
        return View(venue);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var venue = await _context.Venues
            .Include(existingVenue => existingVenue.Bookings)
            .FirstOrDefaultAsync(existingVenue => existingVenue.VenueId == id);

        if (venue == null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (venue.Bookings.Any())
        {
            TempData["ErrorMessage"] = "This venue cannot be deleted because it is linked to one or more bookings.";
            return RedirectToAction(nameof(Index));
        }

        _context.Venues.Remove(venue);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Venue deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
