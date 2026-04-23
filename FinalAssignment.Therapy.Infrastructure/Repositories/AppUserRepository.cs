using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Interfaces;
using FinalAssignment.Therapy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinalAssignment.Therapy.Infrastructure.Repositories;

public class AppUserRepository(TherapyDbContext dbContext) : IAppUserRepository
{
    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await dbContext.Users
            .Include(user => user.TherapistProfile)
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task<AppUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await dbContext.Users
            .Include(user => user.TherapistProfile)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task<AppUser?> GetByTherapistProfileIdAsync(int therapistProfileId, CancellationToken cancellationToken = default)
        => await dbContext.Users
            .Include(user => user.TherapistProfile)
            .FirstOrDefaultAsync(user => user.TherapistProfileId == therapistProfileId, cancellationToken);

    public async Task<bool> AnyByRoleAsync(string role, CancellationToken cancellationToken = default)
        => await dbContext.Users.AnyAsync(user => user.Role == role, cancellationToken);

    public async Task AddAsync(AppUser user, CancellationToken cancellationToken = default)
        => await dbContext.Users.AddAsync(user, cancellationToken);

    public Task UpdateAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);
        return Task.CompletedTask;
    }
}