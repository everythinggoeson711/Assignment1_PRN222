using Microsoft.EntityFrameworkCore;
using Montra.DAL.Context;
using Montra.DAL.Entities;

namespace Montra.DAL.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _db;
        public ServiceRepository(AppDbContext db) => _db = db;

        public async Task<List<TherapyService>> GetAllAsync()
            => await _db.TherapyServices.OrderBy(s => s.Name).ToListAsync();

        public async Task<TherapyService?> GetByIdAsync(int id) => await _db.TherapyServices.FindAsync(id);
        public async Task AddAsync(TherapyService service)      => await _db.TherapyServices.AddAsync(service);
        public Task UpdateAsync(TherapyService service)         { _db.TherapyServices.Update(service); return Task.CompletedTask; }

        public async Task DeleteAsync(int id)
        {
            var s = await _db.TherapyServices.FindAsync(id);
            if (s != null) _db.TherapyServices.Remove(s);
        }

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
    }
}
