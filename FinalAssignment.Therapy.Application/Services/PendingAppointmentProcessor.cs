using FinalAssignment.Therapy.Core.Enums;
using FinalAssignment.Therapy.Core.Interfaces;

namespace FinalAssignment.Therapy.Application.Services;

public class PendingAppointmentProcessor(
    IUnitOfWork unitOfWork,
    IClock clock,
    IAdminDashboardNotifier dashboardNotifier) : IPendingAppointmentProcessor
{
    public async Task<int> ExpirePendingAppointmentsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        var cutoffUtc = clock.UtcNow.Subtract(maxAge);
        var pendingAppointments = await unitOfWork.Appointments.GetPendingPaymentOlderThanAsync(cutoffUtc, cancellationToken);
        if (pendingAppointments.Count == 0)
        {
            return 0;
        }

        foreach (var appointment in pendingAppointments)
        {
            appointment.Status = AppointmentStatus.Expired;
            appointment.PaymentStatus = PaymentStatus.Expired;
            appointment.UpdatedAtUtc = clock.UtcNow;
            await unitOfWork.Appointments.UpdateAsync(appointment, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await dashboardNotifier.BroadcastAsync(cancellationToken);
        return pendingAppointments.Count;
    }
}