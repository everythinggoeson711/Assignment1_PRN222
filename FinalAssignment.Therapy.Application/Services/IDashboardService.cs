using FinalAssignment.Therapy.Application.Models;

namespace FinalAssignment.Therapy.Application.Services;

public interface IDashboardService
{
    Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);
}