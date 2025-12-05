using EventSpark.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSpark.Core.Entities
{
    public class CheckInLog
    {
        public int CheckInLogId { get; set; }

        public int? TicketId { get; set; }
        public int EventId { get; set; }

        public DateTime ScannedAt { get; set; }

        [MaxLength(450)]
        public string? ScannedByUserId { get; set; }

        public CheckInResult Result { get; set; }

        [MaxLength(200)]
        public string? RawCode { get; set; }

        [MaxLength(400)]
        public string? Message { get; set; }

        // Navigation
        public Ticket? Ticket { get; set; }
        public Event Event { get; set; } = null!;
    }
}
