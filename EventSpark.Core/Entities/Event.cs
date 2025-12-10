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

        // filled from Identity User – no need to show in UI
        public string OrganizerId { get; set; } = "";

        public int? CategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(4000, ErrorMessage = "Description is too long.")]
        public string Description { get; set; } = "";

        [Required]
        [Display(Name = "Venue Name")]
        [StringLength(200)]
        public string VenueName { get; set; } = "";

        [Display(Name = "Venue Address")]
        [StringLength(400)]
        public string? VenueAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = "";

        [Required]
        [Display(Name = "Start Date & Time")]
        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; }

        [Required]
        [Display(Name = "End Date & Time")]
        [DataType(DataType.DateTime)]
        public DateTime EndDateTime { get; set; }

        [Required]
        public EventStatus Status { get; set; }

        [Display(Name = "Image Path")]
        [StringLength(400)]
        public string? ImagePath { get; set; }

        [Display(Name = "Max Capacity")]
        [Range(0, 100000, ErrorMessage = "Max capacity must be 0 or greater.")]
        public int? MaxCapacity { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Navigation
        public EventCategory? Category { get; set; }
        public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    }
}
