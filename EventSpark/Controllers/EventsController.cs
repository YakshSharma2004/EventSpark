using System;
using System.Linq;
using System.Threading.Tasks;
using EventSpark.Core.Entities;
using EventSpark.Core.Enums;
using EventSpark.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EventSpark.Web.Models;

namespace EventSpark.Web.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly EventSparkDbContext _db;

        public EventsController(EventSparkDbContext db)
        {
            _db = db;
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            int? categoryId,
            string? city,
            DateTime? from,
            DateTime? to,
            int page = 1,
            int pageSize = 10)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.CurrentUserId = currentUserId;

            // Base query: show published events publicly
            var query = _db.Events
                .Where(e => e.Status == EventStatus.Published)
                .AsQueryable();

            // Text search (title or description)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(e =>
                    e.Title.Contains(term) ||
                    e.Description.Contains(term));
            }

            // Category filter
            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            // City filter
            if (!string.IsNullOrWhiteSpace(city))
            {
                var cityTrimmed = city.Trim();
                query = query.Where(e => e.City == cityTrimmed);
            }

            // Date range (based on StartDateTime)
            if (from.HasValue)
            {
                var fromDate = from.Value.Date;
                query = query.Where(e => e.StartDateTime >= fromDate);
            }

            if (to.HasValue)
            {
                // Include entire "to" day
                var toDateExclusive = to.Value.Date.AddDays(1);
                query = query.Where(e => e.StartDateTime < toDateExclusive);
            }

            // Count before paging
            var totalCount = await query.CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var events = await query
                .OrderBy(e => e.StartDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _db.EventCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            var cities = await _db.Events
                .Select(e => e.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var vm = new EventListViewModel
            {
                SearchTerm = search,
                SelectedCategoryId = categoryId,
                SelectedCity = city,
                FromDate = from,
                ToDate = to,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Events = events,
                Categories = categories,
                Cities = cities
            };

            return View(vm);
        }
        [AllowAnonymous]
        // GET: /Events/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var evt = await _db.Events.FindAsync(id);
            if (evt == null) return NotFound();

            ViewBag.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(evt);
        }
        [Authorize]
        // GET: /Events/Create
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropDownList();

            var model = new Event
            {
                StartDateTime = DateTime.UtcNow.AddDays(7),
                EndDateTime = DateTime.UtcNow.AddDays(7).AddHours(2),
                Status = EventStatus.Draft
            };

            return View(model);
        }

        // POST: /Events/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model)
        {
            // get the current logged-in user's Id from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // should not happen because of [Authorize], but just in case
                return Challenge();
            }

            model.OrganizerId = userId;

            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            if (model.Status == 0)
                model.Status = EventStatus.Draft;

            _db.Events.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: /Events/Edit/5
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var evt = await _db.Events.FindAsync(id);
            if (evt == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || evt.OrganizerId != userId)
            {
                return Forbid(); // 403 if they don't own this event
            }

            return View(evt);
        }


        // POST: /Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Event model)
        {
            var evt = await _db.Events.FindAsync(model.EventId);
            if (evt == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || evt.OrganizerId != userId)
            {
                return Forbid();
            }

            // update editable fields
            evt.Title = model.Title;
            evt.Description = model.Description;
            evt.VenueName = model.VenueName;
            evt.VenueAddress = model.VenueAddress;
            evt.City = model.City;
            evt.StartDateTime = model.StartDateTime;
            evt.EndDateTime = model.EndDateTime;
            evt.Status = model.Status;
            evt.ImagePath = model.ImagePath;
            evt.MaxCapacity = model.MaxCapacity;
            evt.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: /Events/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var evt = await _db.Events.FirstOrDefaultAsync(e => e.EventId == id);
            if (evt == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || evt.OrganizerId != userId)
            {
                return Forbid();
            }

            return View(evt);
        }

        // POST: /Events/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var evt = await _db.Events.FindAsync(id);
            if (evt == null) return NotFound();

            // only organizer who owns it can delete
            if (evt.OrganizerId != userId)
            {
                return Forbid();
            }

            // 1) if any tickets exist, don’t allow delete – event has orders
            var hasTickets = await _db.Tickets.AnyAsync(t => t.EventId == id);
            if (hasTickets)
            {
                TempData["ErrorMessage"] =
                    "This event has existing orders/tickets and cannot be deleted. " +
                    "You can mark it as Cancelled instead.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // 2) delete check-in logs for this event (could have TicketId = NULL)
            var logs = _db.CheckInLogs.Where(l => l.EventId == id);
            _db.CheckInLogs.RemoveRange(logs);

            // 3) delete ticket types for this event
            var ticketTypes = _db.TicketTypes.Where(tt => tt.EventId == id);
            _db.TicketTypes.RemoveRange(ticketTypes);

            // 4) finally delete the event itself
            _db.Events.Remove(evt);

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(MyEvents));
        }
        // GET: /Events/MyEvents
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // Shouldn’t happen because of [Authorize], but just in case:
                return Challenge();
            }

            var myEvents = await _db.Events
                .Where(e => e.OrganizerId == userId)
                .OrderByDescending(e => e.StartDateTime)
                .ToListAsync();
            ViewBag.CurrentUserId = userId;

            return View(myEvents);
        }

        // Helper for dropdown
        private async Task PopulateCategoriesDropDownList(object? selectedCategory = null)
        {
            var categories = await _db.EventCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.CategoryId = new SelectList(categories, "EventCategoryId", "Name", selectedCategory);
        }
    }
}
