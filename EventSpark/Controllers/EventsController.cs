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
        // GET: /Events
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var eventsList = await _db.Events
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            // Logged-in user id (null if anonymous)
            ViewBag.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return View(eventsList);
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
