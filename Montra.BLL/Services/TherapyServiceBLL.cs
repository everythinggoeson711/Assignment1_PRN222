using Montra.DAL.Entities;
using Montra.DAL.Repositories;

namespace Montra.BLL.Services
{
    public class TherapyServiceBLL : ITherapyServiceBLL
    {
        private readonly IServiceRepository _services;
        public TherapyServiceBLL(IServiceRepository services) => _services = services;

        public Task<List<TherapyService>> GetAllAsync()        => _services.GetAllAsync();
        public Task<TherapyService?> GetByIdAsync(int id)      => _services.GetByIdAsync(id);

        public async Task CreateAsync(TherapyService service)
        {
            await _services.AddAsync(service);
            await _services.SaveChangesAsync();
        }

        public async Task UpdateAsync(TherapyService service)
        {
            await _services.UpdateAsync(service);
            await _services.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _services.DeleteAsync(id);
            await _services.SaveChangesAsync();
        }
    }
}
