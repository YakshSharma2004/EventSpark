using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSpark.Web.Models
{
    public class CartLineViewModel
    {
        public int TicketTypeId { get; set; }
        public string Name { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class OrderCartViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = "";
        public DateTime EventStartDateTime { get; set; }
        public string VenueName { get; set; } = "";

        public List<CartLineViewModel> Lines { get; set; } = new();

        public decimal TotalAmount => Lines.Sum(l => l.LineTotal);
    }
}
