using EventEase.Web.Data;
using EventEase.Web.Models;
using EventEase.Web.Models.ViewModels;
using EventEase.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Web.Controllers;

public class VenuesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IImageStorageService _imageStorageService;

    public VenuesController(ApplicationDbContext context, IImageStorageService imageStorageService)
    {
        _context = context;
        _imageStorageService = imageStorageService;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? minCapacity, bool? isAvailable)
    {
        ViewData["SearchTerm"] = searchTerm;
        ViewData["MinCapacity"] = minCapacity;
        ViewData["IsAvailable"] = isAvailable?.ToString().ToLowerInvariant();

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

        if (isAvailable.HasValue)
        {
            venuesQuery = venuesQuery.Where(venue => venue.IsAvailable == isAvailable.Value);
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
        return View(new VenueFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VenueFormViewModel model)
    {
        if (model.ImageFile == null)
        {
            ModelState.AddModelError(nameof(model.ImageFile), "Please upload a venue image.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string imageUrl;

        try
        {
            imageUrl = await _imageStorageService.UploadVenueImageAsync(model.ImageFile!);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
            return View(model);
        }

        var venue = new Venue
        {
            VenueName = model.VenueName,
            Location = model.Location,
            Capacity = model.Capacity,
            IsAvailable = model.IsAvailable,
            ImageUrl = imageUrl
        };

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

        return View(new VenueFormViewModel
        {
            VenueId = venue.VenueId,
            VenueName = venue.VenueName,
            Location = venue.Location,
            Capacity = venue.Capacity,
            IsAvailable = venue.IsAvailable,
            ExistingImageUrl = venue.ImageUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VenueFormViewModel model)
    {
        if (id != model.VenueId)
        {
            return NotFound();
        }

        var existingVenue = await _context.Venues.FindAsync(id);
        if (existingVenue == null)
        {
            return NotFound();
        }

        model.ExistingImageUrl = existingVenue.ImageUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.ImageFile != null)
        {
            try
            {
                existingVenue.ImageUrl = await _imageStorageService.UploadVenueImageAsync(model.ImageFile);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                return View(model);
            }
        }

        existingVenue.VenueName = model.VenueName;
        existingVenue.Location = model.Location;
        existingVenue.Capacity = model.Capacity;
        existingVenue.IsAvailable = model.IsAvailable;

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
