using Montra.DAL.Entities;

namespace Montra.BLL.Services
{
    public interface ITherapyServiceBLL
    {
        Task<List<TherapyService>> GetAllAsync();
        Task<TherapyService?> GetByIdAsync(int id);
        Task CreateAsync(TherapyService service);
        Task UpdateAsync(TherapyService service);
        Task DeleteAsync(int id);
    }
}
