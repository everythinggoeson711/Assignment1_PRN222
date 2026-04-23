using Montra.DAL.Context;
using Montra.DAL.Entities;
using Montra.DAL.Repositories;

namespace Montra.BLL.Services
{
    public class TherapistService : ITherapistService
    {
        private readonly ITherapistRepository _therapists;
        private readonly IUserRepository _users;

        public TherapistService(ITherapistRepository therapists, IUserRepository users)
        {
            _therapists = therapists;
            _users = users;
        }

        public Task<List<Therapist>> GetAllAsync(string? search = null, string? specialty = null)
            => _therapists.GetAllAsync(search, specialty);

        public Task<Therapist?> GetByIdAsync(int id)       => _therapists.GetByIdAsync(id);
        public Task<Therapist?> GetByUserIdAsync(int userId) => _therapists.GetByUserIdAsync(userId);
        public Task<List<string>> GetSpecialtiesAsync()    => _therapists.GetSpecialtiesAsync();

        public async Task CreateAsync(Therapist therapist, User user)
        {
            // Hash password and persist the new user account first
            user.PasswordHash = AppDbContext.Hash(user.PasswordHash);
            user.Role = "Therapist";
            await _users.AddAsync(user);
            await _users.SaveChangesAsync();           // user.Id is now populated

            therapist.UserId = user.Id;
            await _therapists.AddAsync(therapist);
            await _therapists.SaveChangesAsync();
        }

        public async Task UpdateAsync(Therapist therapist)
        {
            await _therapists.UpdateAsync(therapist);
            await _therapists.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _therapists.DeleteAsync(id);
            await _therapists.SaveChangesAsync();
        }
    }
}
