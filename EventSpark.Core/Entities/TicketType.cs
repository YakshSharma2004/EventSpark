using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSpark.Core.Entities
{
    public class TicketType
    {
        public int TicketTypeId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(400)]
        public string? Description { get; set; }

        [Required]
        [Range(0, 999999)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Total Quantity Available")]
        [Range(0, 100000)]
        public int TotalQuantity { get; set; }

        [Display(Name = "Sales Start (UTC)")]
        [DataType(DataType.DateTime)]
        public DateTime? SaleStartUtc { get; set; }

        [Display(Name = "Sales End (UTC)")]
        [DataType(DataType.DateTime)]
        public DateTime? SaleEndUtc { get; set; }

        public DateTime CreatedAt { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Navigation
        public Event Event { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
