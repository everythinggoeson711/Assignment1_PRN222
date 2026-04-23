using FinalAssignment.Therapy.Core.Enums;

namespace FinalAssignment.Therapy.Application.Models;

public class DashboardMetricsDto
{
    public int PendingPaymentCount { get; set; }

    public int ConfirmedCount { get; set; }

    public int CompletedCount { get; set; }

    public int ExpiredCount { get; set; }

    public decimal TotalRevenue { get; set; }

    public IReadOnlyList<AppointmentSummaryDto> RecentAppointments { get; set; } = [];

    public IDictionary<AppointmentStatus, int> StatusBreakdown { get; set; } = new Dictionary<AppointmentStatus, int>();
}