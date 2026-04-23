using Microsoft.EntityFrameworkCore;
using Montra.DAL.Context;
using Montra.DAL.Entities;

namespace Montra.DAL.Repositories
{
    public class TherapistRepository : ITherapistRepository
    {
        private readonly AppDbContext _db;
        public TherapistRepository(AppDbContext db) => _db = db;

        public async Task<List<Therapist>> GetAllAsync(string? search = null, string? specialty = null)
        {
            var q = _db.Therapists.Include(t => t.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(t => t.Name.Contains(search) || t.Specialty.Contains(search));

            if (!string.IsNullOrWhiteSpace(specialty))
                q = q.Where(t => t.Specialty == specialty);

            return await q.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<Therapist?> GetByIdAsync(int id)
            => await _db.Therapists.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Therapist?> GetByUserIdAsync(int userId)
            => await _db.Therapists.Include(t => t.User).FirstOrDefaultAsync(t => t.UserId == userId);

        public async Task<List<string>> GetSpecialtiesAsync()
            => await _db.Therapists.Select(t => t.Specialty).Distinct().OrderBy(s => s).ToListAsync();

        public async Task AddAsync(Therapist therapist) => await _db.Therapists.AddAsync(therapist);
        public Task UpdateAsync(Therapist therapist)    { _db.Therapists.Update(therapist); return Task.CompletedTask; }

        public async Task DeleteAsync(int id)
        {
            var t = await _db.Therapists.FindAsync(id);
            if (t != null) _db.Therapists.Remove(t);
        }

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
    }
}
