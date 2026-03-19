using System.Diagnostics;
using EventEase.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Web.Models;
using EventEase.Web.Models.ViewModels;

namespace EventEase.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;

        var bookedVenueIdsToday = await _context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Event)
            .Where(booking => booking.Event != null
                && booking.Event.EventDate <= today
                && booking.Event.EndDate >= today)
            .Select(booking => booking.VenueId)
            .Distinct()
            .ToListAsync();

        var model = new DashboardViewModel
        {
            VenueCount = await _context.Venues.CountAsync(),
            EventCount = await _context.Events.CountAsync(),
            BookingCount = await _context.Bookings.CountAsync(),
            PendingEventCount = await _context.Events.CountAsync(eventItem => eventItem.VenueId == null),
            AvailableVenueCountToday = await _context.Venues.CountAsync(venue => !bookedVenueIdsToday.Contains(venue.VenueId)),
            UpcomingEvents = await _context.Events
                .AsNoTracking()
                .Include(eventItem => eventItem.Venue)
                .OrderBy(eventItem => eventItem.EventDate)
                .ThenBy(eventItem => eventItem.EventName)
                .Take(5)
                .ToListAsync(),
            RecentBookings = await _context.Bookings
                .AsNoTracking()
                .Include(booking => booking.Event)
                .Include(booking => booking.Venue)
                .OrderByDescending(booking => booking.BookingDate)
                .Take(5)
                .ToListAsync(),
            AvailableVenuesToday = await _context.Venues
                .AsNoTracking()
                .Where(venue => !bookedVenueIdsToday.Contains(venue.VenueId))
                .OrderBy(venue => venue.VenueName)
                .Take(4)
                .ToListAsync()
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
