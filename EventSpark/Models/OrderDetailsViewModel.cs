using System;
using System.Collections.Generic;

namespace EventSpark.Web.Models
{
    public class OrderTicketViewModel
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; } = null!;
        public string TicketTypeName { get; set; } = null!;
    }

    public class OrderDetailsViewModel
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }

        public string EventTitle { get; set; } = null!;
        public DateTime EventStartDateTime { get; set; }
        public string VenueName { get; set; } = null!;

        public List<OrderTicketViewModel> Tickets { get; set; } = new();
    }
}
