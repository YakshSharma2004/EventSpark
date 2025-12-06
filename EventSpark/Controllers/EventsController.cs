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
        public async Task<IActionResult> Index()
        {
            var eventsQuery = _db.Events
                .Include(e => e.Category)
                .OrderBy(e => e.StartDateTime);

            var events = await eventsQuery.ToListAsync();
            return View(events);
        }
        [AllowAnonymous]
        // GET: /Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var evt = await _db.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EventId == id.Value);

            if (evt == null) return NotFound();

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
            var evt = await _db.Events.FindAsync(id);
            if (evt == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || evt.OrganizerId != userId)
            {
                return Forbid();
            }

            _db.Events.Remove(evt);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
