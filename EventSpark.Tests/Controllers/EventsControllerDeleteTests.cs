using System.Security.Claims;
using System.Threading.Tasks;
using EventSpark.Core.Entities;
using EventSpark.Core.Enums;
using EventSpark.Infrastructure.Data;
using EventSpark.Tests.TestHelpers;
using EventSpark.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace EventSpark.Tests.Controllers
{
    public class EventsControllerDeleteTests
    {
        private EventsController CreateController(EventSparkDbContext db, string userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "TestAuth"));

            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            var controller = new EventsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                },
                TempData = new TempDataDictionary(httpContext, new DummyTempDataProvider())
            };

            return controller;
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesEvent_WhenUserIsOwnerAndNoTickets()
        {
            // Arrange
            using var db = TestDbFactory.CreateInMemoryDbContext(nameof(DeleteConfirmed_RemovesEvent_WhenUserIsOwnerAndNoTickets));

            var ownerId = "user-1";

            db.Events.Add(new Event
            {
                EventId = 10,
                Title = "Deletable Event",
                City = "Calgary",
                StartDateTime = System.DateTime.UtcNow,
                EndDateTime = System.DateTime.UtcNow.AddHours(2),
                Status = EventStatus.Draft,
                OrganizerId = ownerId
            });

            await db.SaveChangesAsync();

            var controller = CreateController(db, ownerId);

            // Act
            var result = await controller.DeleteConfirmed(10);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be(nameof(EventsController.MyEvents));

            (await db.Events.FindAsync(10)).Should().BeNull();
        }

        [Fact]
        public async Task DeleteConfirmed_DoesNotDeleteEvent_WhenTicketsExist()
        {
            // Arrange
            using var db = TestDbFactory.CreateInMemoryDbContext(nameof(DeleteConfirmed_DoesNotDeleteEvent_WhenTicketsExist));

            var ownerId = "user-1";

            db.Events.Add(new Event
            {
                EventId = 20,
                Title = "Event With Tickets",
                City = "Calgary",
                StartDateTime = System.DateTime.UtcNow,
                EndDateTime = System.DateTime.UtcNow.AddHours(2),
                Status = EventStatus.Published,
                OrganizerId = ownerId
            });

            db.TicketTypes.Add(new TicketType
            {
                TicketTypeId = 1,
                EventId = 20,
                Name = "GA",
                Price = 50,
                TotalQuantity = 100
            });

            db.Tickets.Add(new Ticket
            {
                TicketId = 1,
                EventId = 20,
                TicketNumber = "EVT20-TT1-ABC",
                Status = TicketStatus.Active
            });

            await db.SaveChangesAsync();

            var controller = CreateController(db, ownerId);

            // Act
            var result = await controller.DeleteConfirmed(20);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be(nameof(EventsController.Details));

            (await db.Events.FindAsync(20)).Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsForbid_WhenUserIsNotOwner()
        {
            // Arrange
            using var db = TestDbFactory.CreateInMemoryDbContext(nameof(DeleteConfirmed_ReturnsForbid_WhenUserIsNotOwner));

            db.Events.Add(new Event
            {
                EventId = 30,
                Title = "Someone Else's Event",
                City = "Calgary",
                StartDateTime = System.DateTime.UtcNow,
                EndDateTime = System.DateTime.UtcNow.AddHours(2),
                Status = EventStatus.Draft,
                OrganizerId = "owner-id"
            });

            await db.SaveChangesAsync();

            var controller = CreateController(db, userId: "different-user");

            // Act
            var result = await controller.DeleteConfirmed(30);

            // Assert
            result.Should().BeOfType<ForbidResult>();

            (await db.Events.FindAsync(30)).Should().NotBeNull();
        }
    }
}
