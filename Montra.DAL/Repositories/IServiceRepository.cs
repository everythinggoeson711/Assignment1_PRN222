using Montra.DAL.Entities;

namespace Montra.DAL.Repositories
{
    public interface IServiceRepository
    {
        Task<List<TherapyService>> GetAllAsync();
        Task<TherapyService?> GetByIdAsync(int id);
        Task AddAsync(TherapyService service);
        Task UpdateAsync(TherapyService service);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }
}
