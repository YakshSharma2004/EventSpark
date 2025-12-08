using System;
using System.Collections.Generic;
using EventSpark.Core.Enums;

namespace EventSpark.Web.Models
{
    public class MyOrderListItemViewModel
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }

        public string EventTitle { get; set; } = "";
        public DateTime EventStartDateTime { get; set; }
        public string VenueName { get; set; } = "";
    }

    public class MyTicketListItemViewModel
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; } = "";
        public string TicketTypeName { get; set; } = "";

        public string EventTitle { get; set; } = "";
        public DateTime EventStartDateTime { get; set; }
        public string VenueName { get; set; } = "";

        public int OrderId { get; set; }
        public TicketStatus Status { get; set; }
    }
}
