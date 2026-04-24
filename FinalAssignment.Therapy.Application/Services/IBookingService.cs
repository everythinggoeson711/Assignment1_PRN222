using FinalAssignment.Therapy.Application.Models;

namespace FinalAssignment.Therapy.Application.Services;

public interface IBookingService
{
    Task<BookingResult> CreateBookingAsync(AppointmentBookingRequest request, CancellationToken cancellationToken = default);
    Task ProcessSePayWebhookAsync(SePayWebhookData data, CancellationToken cancellationToken = default);
}
