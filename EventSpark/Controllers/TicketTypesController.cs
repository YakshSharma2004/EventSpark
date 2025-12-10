using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EventSpark.Core.Auth;
using EventSpark.Core.Entities;
using EventSpark.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventSpark.Web.Controllers
{
    [Authorize]
    public class TicketTypesController : Controller
    {
        private readonly EventSparkDbContext _db;

        public TicketTypesController(EventSparkDbContext db)
        {
            _db = db;
        }

        // Helper: load event and ensure current user can manage it (owner or Admin)
        private async Task<Event?> GetEventUserCanManageAsync(int eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return null;

            var evt = await _db.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt == null) return null;

            if (evt.OrganizerId != userId && !User.IsInRole(AppRole.Admin))
            {
                return null;
            }

            return evt;
        }

        // GET: /TicketTypes/Index?eventId=5
        [HttpGet]
        public async Task<IActionResult> Index(int eventId)
        {
            var evt = await GetEventUserCanManageAsync(eventId);
            if (evt == null) return Forbid();

            var types = await _db.TicketTypes
                .Where(t => t.EventId == eventId)
                .OrderBy(t => t.Price)
                .ToListAsync();

            // ✅ These match the view code we wrote earlier
            ViewBag.EventId = evt.EventId;
            ViewBag.EventTitle = evt.Title;
            ViewBag.EventStart = evt.StartDateTime;

            return View(types);
        }

        // GET: /TicketTypes/Create?eventId=5
        [HttpGet]
        public async Task<IActionResult> Create(int eventId)
        {
            var evt = await GetEventUserCanManageAsync(eventId);
            if (evt == null) return Forbid();

            var model = new TicketType
            {
                EventId = eventId,
                Price = 0,
                TotalQuantity = 100,
                SaleStartUtc = DateTime.UtcNow,
                SaleEndUtc = DateTime.UtcNow.AddMonths(1)
            };

            ViewBag.EventId = evt.EventId;
            ViewBag.EventTitle = evt.Title;
            ViewBag.EventStart = evt.StartDateTime;

            return View(model);
        }

        // POST: /TicketTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketType model)
        {
            var evt = await GetEventUserCanManageAsync(model.EventId);
            if (evt == null) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.EventId = evt.EventId;
                ViewBag.EventTitle = evt.Title;
                ViewBag.EventStart = evt.StartDateTime;
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;

            _db.TicketTypes.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { eventId = model.EventId });
        }

        // GET: /TicketTypes/Edit/10
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tt = await _db.TicketTypes
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.TicketTypeId == id);

            if (tt == null) return NotFound();

            var evt = await GetEventUserCanManageAsync(tt.EventId);
            if (evt == null) return Forbid();

            ViewBag.EventId = evt.EventId;
            ViewBag.EventTitle = evt.Title;
            ViewBag.EventStart = evt.StartDateTime;

            return View(tt);
        }

        // POST: /TicketTypes/Edit/10
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TicketType model)
        {
            var tt = await _db.TicketTypes
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.TicketTypeId == model.TicketTypeId);

            if (tt == null) return NotFound();

            var evt = await GetEventUserCanManageAsync(tt.EventId);
            if (evt == null) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.EventId = evt.EventId;
                ViewBag.EventTitle = evt.Title;
                ViewBag.EventStart = evt.StartDateTime;
                return View(model);
            }

            // update editable fields
            tt.Name = model.Name;
            tt.Description = model.Description;
            tt.Price = model.Price;
            tt.TotalQuantity = model.TotalQuantity;
            tt.SaleStartUtc = model.SaleStartUtc;
            tt.SaleEndUtc = model.SaleEndUtc;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { eventId = tt.EventId });
        }

        // GET: /TicketTypes/Delete/10
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var tt = await _db.TicketTypes
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.TicketTypeId == id);

            if (tt == null) return NotFound();

            var evt = await GetEventUserCanManageAsync(tt.EventId);
            if (evt == null) return Forbid();

            ViewBag.EventId = evt.EventId;
            ViewBag.EventTitle = evt.Title;
            ViewBag.EventStart = evt.StartDateTime;

            return View(tt);
        }

        // POST: /TicketTypes/Delete/10
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tt = await _db.TicketTypes
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.TicketTypeId == id);

            if (tt == null) return NotFound();

            var evt = await GetEventUserCanManageAsync(tt.EventId);
            if (evt == null) return Forbid();

            var eventId = tt.EventId;

            _db.TicketTypes.Remove(tt);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { eventId });
        }
    }
}
