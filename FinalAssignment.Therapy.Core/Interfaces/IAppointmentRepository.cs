using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Enums;

namespace FinalAssignment.Therapy.Core.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Appointment?> GetByTrackingCodeAsync(string trackingCode, CancellationToken cancellationToken = default);

    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetPendingPaymentPastDueAsync(DateTime threshold, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetForTherapistAsync(int therapistProfileId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetPendingPaymentOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);

    Task<int> CountByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default);

    Task<decimal> SumConfirmedRevenueAsync(CancellationToken cancellationToken = default);

    Task<bool> HasConflictAsync(int therapistProfileId, DateTime appointmentStartUtc, CancellationToken cancellationToken = default);
    Task<bool> HasConflictInRangeAsync(int therapistId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken);
    Task RemoveAsync(Appointment appointment, CancellationToken cancellationToken = default);
}