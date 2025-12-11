using System;
using System.Linq;
using System.Threading.Tasks;
using EventSpark.Core.Entities;
using EventSpark.Core.Enums;
using EventSpark.Infrastructure.Data;
using EventSpark.Tests.TestHelpers;
using EventSpark.Web.Controllers;
using EventSpark.Web.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EventSpark.Tests.Controllers
{
    public class EventsControllerIndexTests
    {
        private async Task SeedEventsAsync(EventSparkDbContext db)
        {
            db.Events.AddRange(
                new Event
                {
                    EventId = 1,
                    Title = "Rock Concert",
                    Description = "Live music and bands",
                    City = "Calgary",
                    StartDateTime = new DateTime(2025, 1, 10),
                    EndDateTime = new DateTime(2025, 1, 10, 22, 0, 0),
                    Status = EventStatus.Published
                },
                new Event
                {
                    EventId = 2,
                    Title = "Tech Conference",
                    Description = "Developers and workshops",
                    City = "Edmonton",
                    StartDateTime = new DateTime(2025, 2, 1),
                    EndDateTime = new DateTime(2025, 2, 1, 17, 0, 0),
                    Status = EventStatus.Published
                },
                new Event
                {
                    EventId = 3,
                    Title = "Private Planning Meeting",
                    Description = "Internal only",
                    City = "Calgary",
                    StartDateTime = new DateTime(2025, 3, 5),
                    EndDateTime = new DateTime(2025, 3, 5, 15, 0, 0),
                    Status = EventStatus.Draft   // should never appear in public index
                }
            );

            await db.SaveChangesAsync();
        }

        private EventsController CreateController(EventSparkDbContext db)
        {
            var controller = new EventsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            return controller;
        }

        [Fact]
        public async Task Index_SearchFiltersByTitleOrDescription()
        {
            // Arrange
            using var db = TestDbFactory.CreateInMemoryDbContext(nameof(Index_SearchFiltersByTitleOrDescription));
            await SeedEventsAsync(db);
            var controller = CreateController(db);

            // Act
            var result = await controller.Index(
                search: "Concert",
                categoryId: null,
                city: null,
                from: null,
                to: null,
                page: 1,
                pageSize: 10) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            var vm = result!.Model.Should().BeOfType<EventListViewModel>().Subject;

            vm.Events.Should().HaveCount(1);
            vm.Events.Single().Title.Should().Be("Rock Concert");
        }

        [Fact]
        public async Task Index_CityFilterReturnsOnlyMatchingCity()
        {
            // Arrange
            using var db = TestDbFactory.CreateInMemoryDbContext(nameof(Index_CityFilterReturnsOnlyMatchingCity));
            await SeedEventsAsync(db);
            var controller = CreateController(db);

            // Act
            var result = await controller.Index(
                search: null,
                categoryId: null,
                city: "Calgary",
                from: null,
                to: null,
                page: 1,
                pageSize: 10) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            var vm = result!.Model.Should().BeOfType<EventListViewModel>().Subject;

            vm.Events.Should().OnlyContain(e => e.City == "Calgary");
            // Draft event should not be visible
            vm.Events.Should().OnlyContain(e => e.Status == EventStatus.Published);
        }

        [Fact]
        public async Task Index_DateRangeFiltersByStartDate()
        {
            // Arrange
            using var db = TestDbFactory.CreateInMemoryDbContext(nameof(Index_DateRangeFiltersByStartDate));
            await SeedEventsAsync(db);
            var controller = CreateController(db);

            var from = new DateTime(2025, 2, 1);

            // Act
            var result = await controller.Index(
                search: null,
                categoryId: null,
                city: null,
                from: from,
                to: null,
                page: 1,
                pageSize: 10) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            var vm = result!.Model.Should().BeOfType<EventListViewModel>().Subject;

            vm.Events.Should().HaveCount(1);
            vm.Events.Single().Title.Should().Be("Tech Conference");
        }
    }
}
