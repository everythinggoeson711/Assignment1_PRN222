using FinalAssignment.Therapy.Core.Entities;

namespace FinalAssignment.Therapy.Core.Interfaces;

public interface ITherapistRepository
{
    Task AddAsync(TherapistProfile therapistProfile, CancellationToken cancellationToken = default);

    Task UpdateAsync(TherapistProfile therapistProfile, CancellationToken cancellationToken = default);

    Task<TherapistProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TherapistProfile>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TherapistProfile>> GetActiveAsync(CancellationToken cancellationToken = default);
}