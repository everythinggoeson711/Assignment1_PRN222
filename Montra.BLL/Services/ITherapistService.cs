using Montra.DAL.Entities;

namespace Montra.BLL.Services
{
    public interface ITherapistService
    {
        Task<List<Therapist>> GetAllAsync(string? search = null, string? specialty = null);
        Task<Therapist?> GetByIdAsync(int id);
        Task<Therapist?> GetByUserIdAsync(int userId);
        Task<List<string>> GetSpecialtiesAsync();
        Task CreateAsync(Therapist therapist, User user);
        Task UpdateAsync(Therapist therapist);
        Task DeleteAsync(int id);
    }
}
