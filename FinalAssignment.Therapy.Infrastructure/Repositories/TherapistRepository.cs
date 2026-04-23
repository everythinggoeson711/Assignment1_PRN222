using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Interfaces;
using FinalAssignment.Therapy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinalAssignment.Therapy.Infrastructure.Repositories;

public class TherapistRepository(TherapyDbContext dbContext) : ITherapistRepository
{
    public async Task AddAsync(TherapistProfile therapistProfile, CancellationToken cancellationToken = default)
        => await dbContext.Therapists.AddAsync(therapistProfile, cancellationToken);

    public Task UpdateAsync(TherapistProfile therapistProfile, CancellationToken cancellationToken = default)
    {
        dbContext.Therapists.Update(therapistProfile);
        return Task.CompletedTask;
    }

    public async Task<TherapistProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await dbContext.Therapists.FirstOrDefaultAsync(therapist => therapist.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TherapistProfile>> GetAllAsync(CancellationToken cancellationToken = default)
        => await dbContext.Therapists
            .OrderBy(therapist => therapist.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TherapistProfile>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await dbContext.Therapists
            .Where(therapist => therapist.IsActive)
            .OrderBy(therapist => therapist.Name)
            .ToListAsync(cancellationToken);
}