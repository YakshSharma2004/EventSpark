using System;
using System.Collections.Generic;
using EventSpark.Core.Enums;

namespace EventSpark.Web.Models
{
    public class EventStatsRowViewModel
    {
        public int EventId { get; set; }
        public string Title { get; set; } = "";
        public DateTime StartDateTime { get; set; }
        public EventStatus Status { get; set; }

        public int TicketsSold { get; set; }
        public int TicketsCheckedIn { get; set; }
        public int NoShow => TicketsSold - TicketsCheckedIn;
        public decimal Revenue { get; set; }
    }

    public class OrganizerDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalTicketsSold { get; set; }
        public int TotalTicketsCheckedIn { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<EventStatsRowViewModel> Events { get; set; } = new();
    }

    public class AdminDashboardViewModel : OrganizerDashboardViewModel
    {
        public int TotalUsers { get; set; }   // optional, we can fill later
    }
}
