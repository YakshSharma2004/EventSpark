using System;
using System.Linq;
using System.Threading.Tasks;
using EventSpark.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventSpark.Web.Controllers
{
    public class DiagnosticsController : Controller
    {
        private readonly EventSparkDbContext _db;

        public DiagnosticsController(EventSparkDbContext db)
        {
            _db = db;
        }

        // GET: /Diagnostics/EventsPing
        public async Task<IActionResult> EventsPing()
        {
            try
            {
                var canConnect = await _db.Database.CanConnectAsync();

                // This will run SELECT COUNT(*) FROM [Events]
                var totalEvents = await _db.Events.CountAsync();

                // This will try to select real columns; if mapping is wrong, you’ll get a SQL error.
                var sampleEvent = await _db.Events
                    .OrderBy(e => e.EventId)
                    .FirstOrDefaultAsync();

                var message = $"CanConnect: {canConnect}, " +
                              $"TotalEvents: {totalEvents}";

                if (sampleEvent != null)
                {
                    message +=
                        $", FirstEvent: Id={sampleEvent.EventId}, Title={sampleEvent.Title}, City={sampleEvent.City}";
                }
                else
                {
                    message += ", No events found yet.";
                }

                return Content(message);
            }
            catch (Exception ex)
            {
                // If connection string is wrong or mapping is off, you’ll see it here
                return Content("ERROR talking to DB: " + ex.Message);
            }
        }
    }
}
