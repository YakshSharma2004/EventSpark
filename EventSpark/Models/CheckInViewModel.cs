using EventSpark.Core.Enums;

namespace EventSpark.Web.Models
{
    public class CheckInViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = "";

        // The code scanned from QR (we use QrCodeValue)
        public string Code { get; set; } = "";

        public CheckInResult? Result { get; set; }
        public string ResultMessage { get; set; } = "";

        public string? LastTicketNumber { get; set; }
        public string? LastBuyerEmail { get; set; }
    }
}