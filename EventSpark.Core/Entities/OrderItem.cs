using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSpark.Core.Entities
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }
        public int TicketTypeId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Required, MaxLength(100)]
        public string TicketTypeNameSnapshot { get; set; } = null!;

        // Computed column in DB
        [Column(TypeName = "decimal(10,2)")]
        public decimal LineTotal { get; private set; }

        // Navigation
        public Order Order { get; set; } = null!;
        public TicketType TicketType { get; set; } = null!;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    }
}
