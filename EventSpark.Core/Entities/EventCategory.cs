using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSpark.Core.Entities
{
    public class EventCategory
    {
        public int EventCategoryId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(100)]
        public string? Slug { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<Event> Events { get; set; } = new List<Event>();

    }
}
