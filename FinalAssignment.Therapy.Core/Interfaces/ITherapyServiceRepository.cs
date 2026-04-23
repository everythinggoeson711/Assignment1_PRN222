using FinalAssignment.Therapy.Core.Entities;

namespace FinalAssignment.Therapy.Core.Interfaces;

public interface ITherapyServiceRepository
{
    Task<IReadOnlyList<TherapyService>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TherapyService>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<TherapyService?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(TherapyService therapyService, CancellationToken cancellationToken = default);

    Task UpdateAsync(TherapyService therapyService, CancellationToken cancellationToken = default);
}