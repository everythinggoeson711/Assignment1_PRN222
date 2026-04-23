using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Interfaces;
using FinalAssignment.Therapy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinalAssignment.Therapy.Infrastructure.Repositories;

public class TherapyServiceRepository(TherapyDbContext dbContext) : ITherapyServiceRepository
{
    public async Task<IReadOnlyList<TherapyService>> GetAllAsync(CancellationToken cancellationToken = default)
        => await dbContext.TherapyServices
            .OrderBy(service => service.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TherapyService>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await dbContext.TherapyServices
            .Where(service => service.IsActive)
            .OrderBy(service => service.Name)
            .ToListAsync(cancellationToken);

    public async Task<TherapyService?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await dbContext.TherapyServices.FirstOrDefaultAsync(service => service.Id == id, cancellationToken);

    public async Task AddAsync(TherapyService therapyService, CancellationToken cancellationToken = default)
        => await dbContext.TherapyServices.AddAsync(therapyService, cancellationToken);

    public Task UpdateAsync(TherapyService therapyService, CancellationToken cancellationToken = default)
    {
        dbContext.TherapyServices.Update(therapyService);
        return Task.CompletedTask;
    }
}