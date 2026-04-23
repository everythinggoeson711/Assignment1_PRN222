using FinalAssignment.Therapy.Application.Models;
using FinalAssignment.Therapy.Core.Enums;
using FinalAssignment.Therapy.Core.Interfaces;

namespace FinalAssignment.Therapy.Application.Services;

public class DashboardService(IUnitOfWork unitOfWork, IAppointmentService appointmentService) : IDashboardService
{
    public async Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var pendingPaymentCount = await unitOfWork.Appointments.CountByStatusAsync(AppointmentStatus.PendingPayment, cancellationToken);
        var confirmedCount = await unitOfWork.Appointments.CountByStatusAsync(AppointmentStatus.Confirmed, cancellationToken);
        var completedCount = await unitOfWork.Appointments.CountByStatusAsync(AppointmentStatus.Completed, cancellationToken);
        var expiredCount = await unitOfWork.Appointments.CountByStatusAsync(AppointmentStatus.Expired, cancellationToken);
        var totalRevenue = await unitOfWork.Appointments.SumConfirmedRevenueAsync(cancellationToken);
        var recentAppointments = await appointmentService.GetRecentAsync(6, cancellationToken);

        return new DashboardMetricsDto
        {
            PendingPaymentCount = pendingPaymentCount,
            ConfirmedCount = confirmedCount,
            CompletedCount = completedCount,
            ExpiredCount = expiredCount,
            TotalRevenue = totalRevenue,
            RecentAppointments = recentAppointments,
            StatusBreakdown = new Dictionary<AppointmentStatus, int>
            {
                [AppointmentStatus.PendingPayment] = pendingPaymentCount,
                [AppointmentStatus.Confirmed] = confirmedCount,
                [AppointmentStatus.Completed] = completedCount,
                [AppointmentStatus.Expired] = expiredCount
            }
        };
    }
}