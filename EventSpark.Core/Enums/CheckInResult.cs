using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSpark.Core.Enums
{
    public enum CheckInResult: byte
    {
        Success = 0,
        AlreadyCheckedIn = 1,
        InvalidCode = 2,
        CancelledTicket = 3,
        Other = 4
    }
}
