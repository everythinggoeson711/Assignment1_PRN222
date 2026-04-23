namespace Montra.BLL.Services
{
    public interface ICenterBookingService
    {
        Task<CenterBookingCheckoutResult> CreateCheckoutAsync(CenterBookingInput input);
        Task<(bool Available, string Message)> CheckAvailabilityAsync(int therapistId, DateTime date, string timeSlot);
        Task MarkCheckoutCompletedAsync(string stripeSessionId, string stripePaymentIntentId);
        Task MarkCheckoutExpiredAsync(string stripeSessionId);
        Task MarkCheckoutFailedAsync(int? bookingId, string stripePaymentIntentId, string failureMessage);
        Task MarkCheckoutCanceledAsync(int bookingId);
    }
}
