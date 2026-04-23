using Montra.DAL.Entities;

namespace Montra.BLL.Services
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<User?> GetUserByIdAsync(int id);
    }
}
