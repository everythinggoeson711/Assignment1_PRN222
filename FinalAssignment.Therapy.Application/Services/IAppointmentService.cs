using FinalAssignment.Therapy.Application.Models;

namespace FinalAssignment.Therapy.Application.Services;

public interface IAppointmentService
{
    Task<ServiceResult> BookAsync(AppointmentBookingRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> ConfirmPaymentAsync(int appointmentId, CancellationToken cancellationToken = default);

    Task<ServiceResult> CompleteAsync(int appointmentId, int therapistProfileId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentSummaryDto>> GetRecentAsync(int count, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentSummaryDto>> GetForTherapistAsync(int therapistProfileId, CancellationToken cancellationToken = default);
}