using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EventSpark.Core.Entities;
using EventSpark.Core.Enums;
using EventSpark.Infrastructure.Data;
using EventSpark.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace EventSpark.Web.Controllers
{
    [Authorize] // all order/ticket actions require login
    [Route("[controller]/[action]")]
    public class OrdersController : Controller
    {
        private readonly EventSparkDbContext _db;
        [HttpGet]
        public IActionResult Ping()
        {
            return Content("OrdersController is working (Ping).");
        }
        public OrdersController(EventSparkDbContext db)
        {
            _db = db;
        }

        // ============================
        // 1) SELECT TICKETS (GET)
        // ============================
        // GET: /Orders/Purchase?eventId=5
        [HttpGet]
        public async Task<IActionResult> Purchase(int eventId)
        {
            var evt = await _db.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.Status == EventStatus.Published);

            if (evt == null)
            {
                return NotFound();
            }

            var vm = new PurchaseTicketsViewModel
            {
                EventId = evt.EventId,
                EventTitle = evt.Title,
                EventStartDateTime = evt.StartDateTime,
                VenueName = evt.VenueName
            };

            foreach (var tt in evt.TicketTypes.OrderBy(t => t.Price))
            {
                vm.Tickets.Add(new TicketSelectionViewModel
                {
                    TicketTypeId = tt.TicketTypeId,
                    Name = tt.Name,
                    Price = tt.Price,
                    Quantity = 0
                });
            }

            if (!vm.Tickets.Any())
            {
                ViewBag.Message = "No ticket types are available for this event.";
            }

            return View(vm);   // Views/Orders/Purchase.cshtml
        }

        // ============================
        // 2) BUILD CART (POST Purchase)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(PurchaseTicketsViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var requested = model.Tickets?
                .Where(t => t.Quantity > 0)
                .ToList() ?? new();

            if (!requested.Any())
            {
                ModelState.AddModelError("", "Please select at least one ticket.");
                return await RebuildPurchaseViewWithErrors(model);
            }

            var evt = await _db.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.EventId == model.EventId && e.Status == EventStatus.Published);

            if (evt == null)
            {
                return NotFound();
            }

            var ticketTypeIds = requested.Select(r => r.TicketTypeId).ToList();

            var ticketTypes = evt.TicketTypes
                .Where(tt => ticketTypeIds.Contains(tt.TicketTypeId))
                .ToList();

            if (ticketTypes.Count != ticketTypeIds.Count)
            {
                ModelState.AddModelError("", "One or more selected ticket types are not available.");
                return await RebuildPurchaseViewWithErrors(model);
            }

            var cart = new OrderCartViewModel
            {
                EventId = evt.EventId,
                EventTitle = evt.Title,
                EventStartDateTime = evt.StartDateTime,
                VenueName = evt.VenueName
            };

            foreach (var line in requested)
            {
                var tt = ticketTypes.First(t => t.TicketTypeId == line.TicketTypeId);
                cart.Lines.Add(new CartLineViewModel
                {
                    TicketTypeId = tt.TicketTypeId,
                    Name = tt.Name,
                    UnitPrice = tt.Price,
                    Quantity = line.Quantity
                });
            }

            return View("Cart", cart);  // Views/Orders/Cart.cshtml
        }

        // ============================
        // 3) CONFIRM & PAY (CREATE ORDER)
        // ============================
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ConfirmAndPay(OrderCartViewModel model)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (userId == null) return Challenge();

        //    var requested = model.Lines?
        //        .Where(l => l.Quantity > 0)
        //        .ToList() ?? new();

        //    if (!requested.Any())
        //    {
        //        return RedirectToAction(nameof(Purchase), new { eventId = model.EventId });
        //    }

        //    var evt = await _db.Events
        //        .FirstOrDefaultAsync(e => e.EventId == model.EventId && e.Status == EventStatus.Published);

        //    if (evt == null)
        //    {
        //        return NotFound();
        //    }

        //    var ticketTypeIds = requested.Select(r => r.TicketTypeId).Distinct().ToList();

        //    var ticketTypes = await _db.TicketTypes
        //        .Where(tt => tt.EventId == model.EventId && ticketTypeIds.Contains(tt.TicketTypeId))
        //        .ToListAsync();

        //    if (ticketTypes.Count != ticketTypeIds.Count)
        //    {
        //        return RedirectToAction(nameof(Purchase), new { eventId = model.EventId });
        //    }

        //    decimal total = 0m;
        //    foreach (var line in requested)
        //    {
        //        var tt = ticketTypes.First(t => t.TicketTypeId == line.TicketTypeId);
        //        total += line.Quantity * tt.Price;
        //    }

        //    var order = new Order
        //    {
        //        BuyerId = userId,
        //        CreatedAt = DateTime.UtcNow,
        //        Status = OrderStatus.Completed,     // mock success
        //        TotalAmount = total,
        //        PaymentReference = "TEST-" + Guid.NewGuid().ToString("N").Substring(0, 8),
        //        EmailSnapshot = User.Identity?.Name,
        //        FullNameSnapshot = null
        //    };

        //    _db.Orders.Add(order);

        //    foreach (var line in requested)
        //    {
        //        var tt = ticketTypes.First(t => t.TicketTypeId == line.TicketTypeId);

        //        var orderItem = new OrderItem
        //        {
        //            Order = order,
        //            TicketTypeId = tt.TicketTypeId,
        //            Quantity = line.Quantity,
        //            UnitPrice = tt.Price,
        //            TicketTypeNameSnapshot = tt.Name
        //        };

        //        _db.OrderItems.Add(orderItem);

        //        for (int i = 0; i < line.Quantity; i++)
        //        {
        //            var ticketNumber = $"EVT{tt.EventId}-TT{tt.TicketTypeId}-{Guid.NewGuid():N}".Substring(0, 40);

        //            // Use the ticket number itself as the QR content
        //            var ticket = new Ticket
        //            {
        //                OrderItem = orderItem,
        //                EventId = tt.EventId,
        //                TicketNumber = ticketNumber,
        //                QrCodeValue = ticketNumber, // 🔴 changed
        //                Status = TicketStatus.Active,
        //                CreatedAt = DateTime.UtcNow
        //            };


        //            _db.Tickets.Add(ticket);
        //        }
        //    }

        //    await _db.SaveChangesAsync();

        //    return RedirectToAction(nameof(Payment), new { id = order.OrderId });
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAndPay(OrderCartViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            // Sanity: must have an event
            var evt = await _db.Events.FirstOrDefaultAsync(e => e.EventId == model.EventId);
            if (evt == null) return NotFound();

            // Keep only lines with a quantity
            var selectedLines = (model.Lines ?? new List<CartLineViewModel>())
                .Where(l => l.Quantity > 0)
                .ToList();

            if (!selectedLines.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one ticket.");
            }

            // ==========================
            // Load ticket types for lines
            // ==========================

            var ticketTypeIds = selectedLines
                .Select(l => l.TicketTypeId)
                .Distinct()
                .ToList();

            var ticketTypes = await _db.TicketTypes
                .Where(tt => tt.EventId == model.EventId &&
                             ticketTypeIds.Contains(tt.TicketTypeId))
                .ToListAsync();

            if (ticketTypes.Count != ticketTypeIds.Count)
            {
                ModelState.AddModelError(string.Empty,
                    "One or more selected ticket types are no longer available.");
            }

            // ==========================
            // Inventory check (no oversell)
            // ==========================

            if (ticketTypeIds.Any())
            {
                var soldCounts = await _db.Tickets
                    .Include(t => t.OrderItem)
                    .Where(t => ticketTypeIds.Contains(t.OrderItem.TicketTypeId) &&
                                t.Status == TicketStatus.Active)
                    .GroupBy(t => t.OrderItem.TicketTypeId)
                    .Select(g => new
                    {
                        TicketTypeId = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var soldDict = soldCounts.ToDictionary(x => x.TicketTypeId, x => x.Count);

                foreach (var line in selectedLines)
                {
                    var tt = ticketTypes.FirstOrDefault(t => t.TicketTypeId == line.TicketTypeId);
                    if (tt == null) continue;

                    soldDict.TryGetValue(tt.TicketTypeId, out var alreadySold);

                    var remaining = tt.TotalQuantity - alreadySold;

                    if (remaining <= 0)
                    {
                        ModelState.AddModelError(string.Empty,
                            $"Ticket type '{tt.Name}' is sold out.");
                    }
                    else if (line.Quantity > remaining)
                    {
                        ModelState.AddModelError(string.Empty,
                            $"Cannot buy {line.Quantity} '{tt.Name}' tickets – only {remaining} left.");
                    }
                }
            }

            // If anything failed, show the Cart again with errors
            if (!ModelState.IsValid)
            {
                // Ensure model.Lines only contains the selected lines (with updated quantities)
                model.Lines = selectedLines;
                return View("Cart", model);
            }

            // ==========================
            // Create Order + Tickets
            // ==========================

            // Calculate total using DB prices (trust DB, not the form)
            decimal total = 0m;
            foreach (var line in selectedLines)
            {
                var tt = ticketTypes.First(t => t.TicketTypeId == line.TicketTypeId);
                total += line.Quantity * tt.Price;
            }

            var order = new Order
            {
                BuyerId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Completed, // mock payment succeeded
                TotalAmount = total,
                PaymentReference = "TEST-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                EmailSnapshot = User.Identity?.Name,
                FullNameSnapshot = null
            };

            _db.Orders.Add(order);

            foreach (var line in selectedLines)
            {
                var tt = ticketTypes.First(t => t.TicketTypeId == line.TicketTypeId);

                var orderItem = new OrderItem
                {
                    Order = order,
                    TicketTypeId = tt.TicketTypeId,
                    Quantity = line.Quantity,
                    UnitPrice = tt.Price,
                    TicketTypeNameSnapshot = tt.Name
                };

                _db.OrderItems.Add(orderItem);

                for (int i = 0; i < line.Quantity; i++)
                {
                    var ticketNumber = $"EVT{tt.EventId}-TT{tt.TicketTypeId}-{Guid.NewGuid():N}".Substring(0, 40);

                    var ticket = new Ticket
                    {
                        OrderItem = orderItem,
                        EventId = tt.EventId,
                        TicketNumber = ticketNumber,
                        QrCodeValue = ticketNumber, // we decided QR encodes the ticket number
                        Status = TicketStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.Tickets.Add(ticket);
                }
            }

            await _db.SaveChangesAsync();

            // Go to the fake payment / success screen
            return RedirectToAction(nameof(Payment), new { id = order.OrderId });
        }



        // ============================
        // 4) MOCK PAYMENT SCREEN
        // ============================
        [HttpGet]
        public async Task<IActionResult> Payment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.OrderId == id && o.BuyerId == userId);

            if (order == null) return NotFound();

            var vm = new PaymentStatusViewModel
            {
                OrderId = order.OrderId,
                TotalAmount = order.TotalAmount
            };

            return View(vm);   // Views/Orders/Payment.cshtml
        }

        // ============================
        // 5) ORDER DETAILS
        // ============================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var order = await _db.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Tickets)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.BuyerId == userId);

            if (order == null) return NotFound();

            var anyTicket = order.Items.SelectMany(i => i.Tickets).FirstOrDefault();
            string eventTitle = "Event";
            string venueName = "";
            DateTime eventStart = DateTime.MinValue;

            if (anyTicket != null)
            {
                var evt = await _db.Events.FindAsync(anyTicket.EventId);
                if (evt != null)
                {
                    eventTitle = evt.Title;
                    venueName = evt.VenueName;
                    eventStart = evt.StartDateTime;
                }
            }

            var vm = new OrderDetailsViewModel
            {
                OrderId = order.OrderId,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                EventTitle = eventTitle,
                VenueName = venueName,
                EventStartDateTime = eventStart
            };

            vm.Tickets = order.Items
                .SelectMany(i => i.Tickets.Select(t => new OrderTicketViewModel
                {
                    TicketId = t.TicketId,
                    TicketNumber = t.TicketNumber,
                    TicketTypeName = i.TicketTypeNameSnapshot
                }))
                .ToList();

            return View(vm);
        }

        // ============================
        // 6) MY ORDERS
        // ============================
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var orders = await _db.Orders
                .Where(o => o.BuyerId == userId)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Tickets)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var eventIds = orders
                .SelectMany(o => o.Items)
                .SelectMany(oi => oi.Tickets)
                .Select(t => t.EventId)
                .Distinct()
                .ToList();

            var eventsDict = await _db.Events
                .Where(e => eventIds.Contains(e.EventId))
                .ToDictionaryAsync(e => e.EventId);

            var list = new List<MyOrderListItemViewModel>();

            foreach (var o in orders)
            {
                var anyTicket = o.Items.SelectMany(i => i.Tickets).FirstOrDefault();
                string title = "Event";
                string venue = "";
                DateTime start = DateTime.MinValue;

                if (anyTicket != null && eventsDict.TryGetValue(anyTicket.EventId, out var evt))
                {
                    title = evt.Title;
                    venue = evt.VenueName;
                    start = evt.StartDateTime;
                }

                list.Add(new MyOrderListItemViewModel
                {
                    OrderId = o.OrderId,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalAmount,
                    EventTitle = title,
                    VenueName = venue,
                    EventStartDateTime = start
                });
            }

            return View(list);
        }

        // ============================
        // 7) MY TICKETS
        // ============================
        [HttpGet]
        public async Task<IActionResult> MyTickets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var tickets = await _db.Tickets
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Order)
                .Where(t => t.OrderItem.Order.BuyerId == userId)
                .OrderBy(t => t.EventId)
                .ThenBy(t => t.TicketId)
                .ToListAsync();

            var eventIds = tickets
                .Select(t => t.EventId)
                .Distinct()
                .ToList();

            var eventsDict = await _db.Events
                .Where(e => eventIds.Contains(e.EventId))
                .ToDictionaryAsync(e => e.EventId);

            var list = new List<MyTicketListItemViewModel>();

            foreach (var t in tickets)
            {
                if (!eventsDict.TryGetValue(t.EventId, out var evt))
                    continue;

                list.Add(new MyTicketListItemViewModel
                {
                    TicketId = t.TicketId,
                    TicketNumber = t.TicketNumber,
                    TicketTypeName = t.OrderItem.TicketTypeNameSnapshot,
                    EventTitle = evt.Title,
                    EventStartDateTime = evt.StartDateTime,
                    VenueName = evt.VenueName,
                    OrderId = t.OrderItem.OrderId,
                    Status = t.Status
                });
            }

            return View(list);
        }

        // ============================
        // Helper for validation error case
        // ============================
        private async Task<IActionResult> RebuildPurchaseViewWithErrors(PurchaseTicketsViewModel postedModel)
        {
            var evt = await _db.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.EventId == postedModel.EventId);

            if (evt == null)
            {
                return NotFound();
            }

            var vm = new PurchaseTicketsViewModel
            {
                EventId = evt.EventId,
                EventTitle = evt.Title,
                EventStartDateTime = evt.StartDateTime,
                VenueName = evt.VenueName
            };

            foreach (var tt in evt.TicketTypes.OrderBy(t => t.Price))
            {
                var postedLine = postedModel.Tickets?
                    .FirstOrDefault(p => p.TicketTypeId == tt.TicketTypeId);

                vm.Tickets.Add(new TicketSelectionViewModel
                {
                    TicketTypeId = tt.TicketTypeId,
                    Name = tt.Name,
                    Price = tt.Price,
                    Quantity = postedLine?.Quantity ?? 0
                });
            }

            return View("Purchase", vm);
        }

        // ============================
        // 8) TICKET QR CODE IMAGE
        // ============================
        [HttpGet]
        public async Task<IActionResult> TicketQr(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            // Only allow the ticket owner to view the QR
            var ticket = await _db.Tickets
                .Include(t => t.OrderItem)
                    .ThenInclude(oi => oi.Order)
                .FirstOrDefaultAsync(t => t.TicketId == id &&
                                          t.OrderItem.Order.BuyerId == userId);

            if (ticket == null) return NotFound();

            using var qrGenerator = new QRCodeGenerator();
            // We encode the QrCodeValue stored in DB
            var payload = ticket.TicketNumber; // or ticket.QrCodeValue (same now)
            var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            byte[] bytes = qrCode.GetGraphic(20); // 20 = pixel size

            return File(bytes, "image/png");
        }

    }
}
