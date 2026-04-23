namespace Montra.BLL.Services
{
    public class CenterBookingSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public int PendingPaymentExpiryMinutes { get; set; } = 15;
    }
}
