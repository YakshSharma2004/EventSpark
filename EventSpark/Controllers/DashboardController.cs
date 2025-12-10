using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EventSpark.Core.Entities;
using EventSpark.Core.Enums;
using EventSpark.Infrastructure.Data;
using EventSpark.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventSpark.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly EventSparkDbContext _db;

        public DashboardController(EventSparkDbContext db)
        {
            _db = db;
        }

        // =======================
        // Organizer dashboard
        // =======================
        [HttpGet]
        public async Task<IActionResult> My()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            // All events owned by this organizer
            var events = await _db.Events
                .Where(e => e.OrganizerId == userId)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            var eventIds = events.Select(e => e.EventId).ToList();

            // All tickets for those events
            var tickets = await _db.Tickets
                .Include(t => t.OrderItem)
                .Where(t => eventIds.Contains(t.EventId))
                .ToListAsync();

            var ticketsByEvent = tickets
                .GroupBy(t => t.EventId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var vm = new OrganizerDashboardViewModel();

            foreach (var ev in events)
            {
                ticketsByEvent.TryGetValue(ev.EventId, out var evTickets);
                evTickets ??= new List<Ticket>();

                // You can tweak which statuses count as "sold"
                var sold = evTickets.Count(t => t.Status == TicketStatus.Active);
                var checkedIn = evTickets.Count(t => t.CheckedInAt != null);
                var revenue = evTickets
                    .Where(t => t.Status == TicketStatus.Active)
                    .Sum(t => t.OrderItem.UnitPrice);

                vm.Events.Add(new EventStatsRowViewModel
                {
                    EventId = ev.EventId,
                    Title = ev.Title,
                    StartDateTime = ev.StartDateTime,
                    Status = ev.Status,
                    TicketsSold = sold,
                    TicketsCheckedIn = checkedIn,
                    Revenue = revenue
                });
            }

            vm.TotalEvents = vm.Events.Count;
            vm.TotalTicketsSold = vm.Events.Sum(x => x.TicketsSold);
            vm.TotalTicketsCheckedIn = vm.Events.Sum(x => x.TicketsCheckedIn);
            vm.TotalRevenue = vm.Events.Sum(x => x.Revenue);

            return View(vm);
        }

        // =======================
        // Admin dashboard (all events)
        // =======================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Admin()
        {
            var events = await _db.Events
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            var eventIds = events.Select(e => e.EventId).ToList();

            var tickets = await _db.Tickets
                .Include(t => t.OrderItem)
                .Where(t => eventIds.Contains(t.EventId))
                .ToListAsync();

            var ticketsByEvent = tickets
                .GroupBy(t => t.EventId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var vm = new AdminDashboardViewModel();

            foreach (var ev in events)
            {
                ticketsByEvent.TryGetValue(ev.EventId, out var evTickets);
                evTickets ??= new List<Ticket>();

                var sold = evTickets.Count(t => t.Status == TicketStatus.Active);
                var checkedIn = evTickets.Count(t => t.CheckedInAt != null);
                var revenue = evTickets
                    .Where(t => t.Status == TicketStatus.Active)
                    .Sum(t => t.OrderItem.UnitPrice);

                vm.Events.Add(new EventStatsRowViewModel
                {
                    EventId = ev.EventId,
                    Title = ev.Title,
                    StartDateTime = ev.StartDateTime,
                    Status = ev.Status,
                    TicketsSold = sold,
                    TicketsCheckedIn = checkedIn,
                    Revenue = revenue
                });
            }

            vm.TotalEvents = vm.Events.Count;
            vm.TotalTicketsSold = vm.Events.Sum(x => x.TicketsSold);
            vm.TotalTicketsCheckedIn = vm.Events.Sum(x => x.TicketsCheckedIn);
            vm.TotalRevenue = vm.Events.Sum(x => x.Revenue);

            // We'll fill TotalUsers later when we wire roles/UserManager
            return View(vm);
        }

        // =======================
        // CSV export per event
        // =======================
        [HttpGet]
        public async Task<IActionResult> ExportEventTicketsCsv(int eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var evt = await _db.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt == null) return NotFound();

            // Only the organizer or an Admin can export
            if (evt.OrganizerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var tickets = await _db.Tickets
                .Include(t => t.Event)
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Order)
                .Where(t => t.EventId == eventId)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("TicketId,TicketNumber,Event,TicketType,Price,BuyerEmail,Status,CheckedInAt");

            string Csv(string? s) => $"\"{(s ?? "").Replace("\"", "\"\"")}\"";

            foreach (var t in tickets)
            {
                var price = t.OrderItem.UnitPrice.ToString("0.00");
                var email = t.OrderItem.Order?.EmailSnapshot ?? "";
                var statusStr = t.Status.ToString();
                var checkedInAt = t.CheckedInAt?.ToString("u") ?? "";

                sb.AppendLine(string.Join(",",
                    t.TicketId,
                    Csv(t.TicketNumber),
                    Csv(t.Event.Title),
                    Csv(t.OrderItem.TicketTypeNameSnapshot),
                    price,
                    Csv(email),
                    Csv(statusStr),
                    Csv(checkedInAt)
                ));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"event-{eventId}-tickets.csv";

            return File(bytes, "text/csv", fileName);
        }
    }
}
