using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EventSpark.Core.Auth;
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
    public class CheckInController : Controller
    {
        private readonly EventSparkDbContext _db;

        public CheckInController(EventSparkDbContext db)
        {
            _db = db;
        }

        // GET: /CheckIn?eventId=5
        //[HttpGet]
        //public async Task<IActionResult> Index(int eventId)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (userId == null) return Challenge();

        //    var evt = await _db.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
        //    if (evt == null) return NotFound();

        //    // Only organizer of this event can check in
        //    if (evt.OrganizerId != userId)
        //        return Forbid();

        //    var vm = new CheckInViewModel
        //    {
        //        EventId = evt.EventId,
        //        EventTitle = evt.Title
        //    };

        //    return View(vm);
        //}
        [HttpGet]
        public IActionResult Index()
        {
            // empty form
            return View(new CheckInViewModel());
        }


        // POST: /CheckIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckInViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var rawCode = model.Code?.Trim();

            if (string.IsNullOrEmpty(rawCode))
            {
                model.Result = CheckInResult.Other;
                model.ResultMessage = "Please scan or enter a ticket code.";
                return View(model);
            }

            // Look up by TicketNumber only
            var ticket = await _db.Tickets
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Order)
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.TicketNumber == rawCode);

            var log = new CheckInLog
            {
                EventId = ticket?.EventId ?? 0,
                RawCode = rawCode,
                ScannedAt = DateTime.UtcNow,
                ScannedByUserId = userId
            };

            if (ticket == null)
            {
                log.Result = CheckInResult.InvalidCode;
                log.Message = "Ticket not found.";

                _db.CheckInLogs.Add(log);
                await _db.SaveChangesAsync();

                model.Result = CheckInResult.InvalidCode;
                model.ResultMessage = "Invalid code – ticket not found.";
                return View(model);
            }

            // Only organizer of that event can check in
            if (ticket.Event.OrganizerId != userId)
            {
                return Forbid();
            }

            model.EventId = ticket.EventId;
            model.EventTitle = ticket.Event.Title;

            log.TicketId = ticket.TicketId;

            if (ticket.Status != TicketStatus.Active)
            {
                log.Result = CheckInResult.CancelledTicket;
                log.Message = $"Ticket is not active (status: {ticket.Status}).";

                _db.CheckInLogs.Add(log);
                await _db.SaveChangesAsync();

                model.Result = CheckInResult.CancelledTicket;
                model.ResultMessage = $"Ticket {ticket.TicketNumber} is not active (status: {ticket.Status}).";
                return View(model);
            }

            if (ticket.CheckedInAt != null)
            {
                log.Result = CheckInResult.AlreadyCheckedIn;
                log.Message = "Ticket already checked in.";

                _db.CheckInLogs.Add(log);
                await _db.SaveChangesAsync();

                model.Result = CheckInResult.AlreadyCheckedIn;
                model.ResultMessage = $"Ticket {ticket.TicketNumber} already checked in at {ticket.CheckedInAt}.";
                model.LastTicketNumber = ticket.TicketNumber;
                model.LastBuyerEmail = ticket.OrderItem.Order.EmailSnapshot;

                return View(model);
            }

            // SUCCESS
            ticket.CheckedInAt = DateTime.UtcNow;
            ticket.CheckedInByUserId = userId;

            log.Result = CheckInResult.Success;
            log.Message = "Check-in success.";

            _db.CheckInLogs.Add(log);
            await _db.SaveChangesAsync();

            model.Result = CheckInResult.Success;
            model.ResultMessage = $"Success! Ticket {ticket.TicketNumber} checked in.";
            model.LastTicketNumber = ticket.TicketNumber;
            model.LastBuyerEmail = ticket.OrderItem.Order.EmailSnapshot;
            model.Code = ""; // clear textbox

            return View(model);
        }
    }
}