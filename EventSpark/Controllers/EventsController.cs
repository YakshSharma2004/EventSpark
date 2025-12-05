using System;
using System.Linq;
using System.Threading.Tasks;
using EventSpark.Core.Entities;
using EventSpark.Core.Enums;
using EventSpark.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventSpark.Web.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventSparkDbContext _db;

        public EventsController(EventSparkDbContext db)
        {
            _db = db;
        }

        // GET: /Events
        public async Task<IActionResult> Index()
        {
            var eventsQuery = _db.Events
                .Include(e => e.Category)
                .OrderBy(e => e.StartDateTime);

            var events = await eventsQuery.ToListAsync();
            return View(events);
        }

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropDownList(model.CategoryId);
                return View(model);
            }

            // TODO: replace with real logged-in user (Organizer) once Identity is added
            model.OrganizerId = "demo-organizer";

            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            _db.Events.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var evt = await _db.Events.FindAsync(id.Value);
            if (evt == null) return NotFound();

            await PopulateCategoriesDropDownList(evt.CategoryId);
            return View(evt);
        }

        // POST: /Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, byte[] rowVersion)
        {
            var evtToUpdate = await _db.Events.FirstOrDefaultAsync(e => e.EventId == id);
            if (evtToUpdate == null) return NotFound();

            // TryUpdateModelAsync protects against overposting
            if (await TryUpdateModelAsync(evtToUpdate, "",
                    e => e.Title,
                    e => e.Description,
                    e => e.VenueName,
                    e => e.VenueAddress,
                    e => e.City,
                    e => e.CategoryId,
                    e => e.StartDateTime,
                    e => e.EndDateTime,
                    e => e.Status,
                    e => e.ImagePath,
                    e => e.MaxCapacity))
            {
                evtToUpdate.UpdatedAt = DateTime.UtcNow;

                // Concurrency: attach original RowVersion
                _db.Entry(evtToUpdate).Property("RowVersion").OriginalValue = rowVersion;

                try
                {
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError(
                        "",
                        "This event was modified by another user. Please reload the page and try again."
                    );
                }
            }

            await PopulateCategoriesDropDownList(evtToUpdate.CategoryId);
            return View(evtToUpdate);
        }

        // GET: /Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var evt = await _db.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EventId == id.Value);

            if (evt == null) return NotFound();

            return View(evt);
        }

        // POST: /Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evt = await _db.Events.FindAsync(id);
            if (evt != null)
            {
                _db.Events.Remove(evt);
                await _db.SaveChangesAsync();
            }

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
