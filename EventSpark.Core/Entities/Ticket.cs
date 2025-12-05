using EventSpark.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSpark.Core.Entities
{
    public class Ticket
    {
        public int TicketId { get; set; }

        public int OrderItemId { get; set; }
        public int EventId { get; set; }

        [Required, MaxLength(50)]
        public string TicketNumber { get; set; } = null!;

        [Required, MaxLength(200)]
        public string QrCodeValue { get; set; } = null!;

        public TicketStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? CheckedInAt { get; set; }

        [MaxLength(450)]
        public string? CheckedInByUserId { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // Navigation
        public OrderItem OrderItem { get; set; } = null!;
        public Event Event { get; set; } = null!;
        public ICollection<CheckInLog> CheckInLogs { get; set; } = new List<CheckInLog>();
    }
}
