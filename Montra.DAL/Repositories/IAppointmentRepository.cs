using Montra.DAL.Entities;

namespace Montra.DAL.Repositories
{
    public interface IAppointmentRepository
    {
        Task<List<Appointment>> GetAllAsync(string? patientName = null, int? therapistId = null, AppointmentStatus? status = null, DateTime? date = null);
        Task<Appointment?> GetByIdAsync(int id);
        Task<List<Appointment>> GetByTherapistIdAsync(int therapistId);
        Task<List<Appointment>> GetTodayByTherapistIdAsync(int therapistId);
        Task<bool> HasConflictAsync(int therapistId, DateTime date, string timeSlot, int? excludeId = null);
        Task AddAsync(Appointment appointment);
        Task UpdateAsync(Appointment appointment);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }
}
