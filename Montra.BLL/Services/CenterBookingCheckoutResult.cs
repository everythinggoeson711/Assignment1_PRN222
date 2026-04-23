namespace Montra.BLL.Services
{
    public class CenterBookingCheckoutResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string CheckoutUrl { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
    }
}
