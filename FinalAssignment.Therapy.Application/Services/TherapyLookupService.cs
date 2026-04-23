using FinalAssignment.Therapy.Application.Models;
using FinalAssignment.Therapy.Core.Interfaces;

namespace FinalAssignment.Therapy.Application.Services;

public class TherapyLookupService(IUnitOfWork unitOfWork) : ITherapyLookupService
{
    public async Task<IReadOnlyList<TherapistSummaryDto>> GetActiveTherapistsAsync(CancellationToken cancellationToken = default)
    {
        var therapists = await unitOfWork.Therapists.GetActiveAsync(cancellationToken);
        return therapists.Select(therapist => new TherapistSummaryDto
        {
            Id = therapist.Id,
            Name = therapist.Name,
            Specialty = therapist.Specialty
        }).ToList();
    }

    public async Task<IReadOnlyList<TherapyServiceSummaryDto>> GetActiveServicesAsync(CancellationToken cancellationToken = default)
    {
        var services = await unitOfWork.TherapyServices.GetActiveAsync(cancellationToken);
        return services.Select(service => new TherapyServiceSummaryDto
        {
            Id = service.Id,
            Name = service.Name,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes
        }).ToList();
    }
}