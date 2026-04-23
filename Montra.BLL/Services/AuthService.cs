using Montra.DAL.Context;
using Montra.DAL.Entities;
using Montra.DAL.Repositories;

namespace Montra.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        public AuthService(IUserRepository users) => _users = users;

        public async Task<User?> LoginAsync(string email, string password)
        {
            var hash = AppDbContext.Hash(password);
            var user = await _users.GetByEmailAsync(email);
            return (user != null && user.PasswordHash == hash) ? user : null;
        }

        public async Task<User?> GetUserByIdAsync(int id) => await _users.GetByIdAsync(id);
    }
}
