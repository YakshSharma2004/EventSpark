using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSpark.Core.Enums
{
    public enum TicketStatus:byte
    {
        Active = 0,
        Cancelled = 1,
        Refunded= 2
    }
}
