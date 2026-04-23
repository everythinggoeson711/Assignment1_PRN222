using Montra.DAL.Entities;

namespace Montra.DAL.Repositories
{
    public interface ITherapistRepository
    {
        Task<List<Therapist>> GetAllAsync(string? search = null, string? specialty = null);
        Task<Therapist?> GetByIdAsync(int id);
        Task<Therapist?> GetByUserIdAsync(int userId);
        Task<List<string>> GetSpecialtiesAsync();
        Task AddAsync(Therapist therapist);
        Task UpdateAsync(Therapist therapist);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }
}
