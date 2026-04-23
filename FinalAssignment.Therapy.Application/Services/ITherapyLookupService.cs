using FinalAssignment.Therapy.Application.Models;

namespace FinalAssignment.Therapy.Application.Services;

public interface ITherapyLookupService
{
    Task<IReadOnlyList<TherapistSummaryDto>> GetActiveTherapistsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TherapyServiceSummaryDto>> GetActiveServicesAsync(CancellationToken cancellationToken = default);
}