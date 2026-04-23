using FinalAssignment.Therapy.Core.Entities;

namespace FinalAssignment.Therapy.Core.Interfaces;

public interface IAppUserRepository
{
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<AppUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<AppUser?> GetByTherapistProfileIdAsync(int therapistProfileId, CancellationToken cancellationToken = default);

    Task<bool> AnyByRoleAsync(string role, CancellationToken cancellationToken = default);

    Task AddAsync(AppUser user, CancellationToken cancellationToken = default);

    Task UpdateAsync(AppUser user, CancellationToken cancellationToken = default);
}