using EventSpark.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSpark.Core.Entities
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required, MaxLength(450)]
        public string BuyerId { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public OrderStatus Status { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(100)]
        public string? PaymentReference { get; set; }

        [MaxLength(256)]
        public string? EmailSnapshot { get; set; }

        [MaxLength(200)]
        public string? FullNameSnapshot { get; set; }

        // Navigation
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    }
}
