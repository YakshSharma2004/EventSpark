using System;
using System.Collections.Generic;

namespace EventSpark.Web.Models
{
    public class TicketSelectionViewModel
    {
        public int TicketTypeId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }

        // What the user picks in the form
        public int Quantity { get; set; }
    }

    public class PurchaseTicketsViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = null!;
        public DateTime EventStartDateTime { get; set; }
        public string VenueName { get; set; } = null!;

        public List<TicketSelectionViewModel> Tickets { get; set; } = new();
    }
}
