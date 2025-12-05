using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSpark.Core.Enums
{
    public enum OrderStatus : byte
    {
        Pending = 0,
        Completed = 1,
        Cancelled = 2,
        Refunded = 3
    }
}
