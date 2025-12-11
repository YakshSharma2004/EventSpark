using System;
using System.Collections.Generic;
using EventSpark.Web.Models;
using FluentAssertions;
using Xunit;

namespace EventSpark.Tests.ViewModels
{
    public class OrderCartViewModelTests
    {
        [Fact]
        public void CartLineViewModel_LineTotal_ComputedCorrectly()
        {
            // Arrange
            var line = new CartLineViewModel
            {
                TicketTypeId = 1,
                Name = "General Admission",
                UnitPrice = 50m,
                Quantity = 3
            };

            // Act
            var total = line.LineTotal;

            // Assert
            total.Should().Be(150m);
        }

        [Fact]
        public void OrderCartViewModel_TotalAmount_SumsLineTotals()
        {
            // Arrange
            var cart = new OrderCartViewModel
            {
                EventId = 10,
                EventTitle = "Test Event",
                Lines = new List<CartLineViewModel>
                {
                    new CartLineViewModel { UnitPrice = 20m, Quantity = 2 }, // 40
                    new CartLineViewModel { UnitPrice = 15.5m, Quantity = 1 }, // 15.5
                    new CartLineViewModel { UnitPrice = 5m, Quantity = 3 }     // 15
                }
            };

            // Act
            var total = cart.TotalAmount;

            // Assert
            total.Should().Be(70.5m);
        }
    }
}
