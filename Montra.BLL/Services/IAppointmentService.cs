using Montra.DAL.Entities;

namespace Montra.BLL.Services
{
    public interface IAppointmentService
    {
        Task<List<Appointment>> GetAllAsync(string? patientName = null, int? therapistId = null, AppointmentStatus? status = null, DateTime? date = null);
        Task<Appointment?> GetByIdAsync(int id);
        Task<List<Appointment>> GetByTherapistIdAsync(int therapistId);
        Task<List<Appointment>> GetTodayByTherapistIdAsync(int therapistId);

        /// <summary>Workflow 1 – Create with conflict validation.</summary>
        Task<(bool Success, string Message)> CreateAsync(Appointment appointment);

        /// <summary>Workflow 1 – Update status; re-validates conflict when Confirming.</summary>
        Task<(bool Success, string Message)> UpdateStatusAsync(int id, AppointmentStatus status, string? notes = null);

        Task DeleteAsync(int id);
        Task<int> CountByStatusAsync(AppointmentStatus status);
        Task<decimal> TotalRevenueAsync();
    }
}
