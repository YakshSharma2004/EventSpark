using EventSpark.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSpark.Core.Entities
{
    public class Event
    {
        public int EventId { get; set; }

        // Will map to AspNetUsers.Id later
        [Required, MaxLength(450)]
        public string OrganizerId { get; set; } = null!;

        public int? CategoryId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required, MaxLength(200)]
        public string VenueName { get; set; } = null!;

        [MaxLength(400)]
        public string? VenueAddress { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; } = null!;

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public EventStatus Status { get; set; }

        [MaxLength(400)]
        public string? ImagePath { get; set; }

        public int? MaxCapacity { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // Navigation
        public EventCategory? Category { get; set; }
        public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    }
}
