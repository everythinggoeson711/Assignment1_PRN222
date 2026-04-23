using Microsoft.EntityFrameworkCore;
using Montra.DAL.Context;
using Montra.DAL.Entities;
using Montra.DAL.Repositories;

namespace Montra.BLL.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointments;
        private readonly AppDbContext _db;

        public AppointmentService(IAppointmentRepository appointments, AppDbContext db)
        {
            _appointments = appointments;
            _db = db;
        }

        public Task<List<Appointment>> GetAllAsync(string? patientName = null, int? therapistId = null, AppointmentStatus? status = null, DateTime? date = null)
            => _appointments.GetAllAsync(patientName, therapistId, status, date);

        public Task<Appointment?> GetByIdAsync(int id)                     => _appointments.GetByIdAsync(id);
        public Task<List<Appointment>> GetByTherapistIdAsync(int id)       => _appointments.GetByTherapistIdAsync(id);
        public Task<List<Appointment>> GetTodayByTherapistIdAsync(int id)  => _appointments.GetTodayByTherapistIdAsync(id);

        // ── Workflow 1 Step 1: Create ──────────────────────────────
        public async Task<(bool Success, string Message)> CreateAsync(Appointment appointment)
        {
            bool conflict = await _appointments.HasConflictAsync(
                appointment.TherapistId, appointment.AppointmentDate, appointment.TimeSlot);

            if (conflict)
                return (false, $"Chuyên gia đã có lịch hẹn vào khung giờ {appointment.TimeSlot} ngày {appointment.AppointmentDate:dd/MM/yyyy}. Vui lòng chọn giờ khác.");

            appointment.Status = AppointmentStatus.Pending;
            appointment.CreatedAt = DateTime.Now;
            await _appointments.AddAsync(appointment);
            await _appointments.SaveChangesAsync();
            return (true, "Đặt lịch thành công!");
        }

        // ── Workflow 1 Step 2-4: Update status ────────────────────
        public async Task<(bool Success, string Message)> UpdateStatusAsync(int id, AppointmentStatus status, string? notes = null)
        {
            var appt = await _appointments.GetByIdAsync(id);
            if (appt == null) return (false, "Không tìm thấy lịch hẹn.");

            // Conflict check when therapist confirms
            if (status == AppointmentStatus.Confirmed)
            {
                bool conflict = await _appointments.HasConflictAsync(
                    appt.TherapistId, appt.AppointmentDate, appt.TimeSlot, excludeId: id);

                if (conflict)
                    return (false, $"Xung đột lịch hẹn: khung giờ {appt.TimeSlot} đã được xác nhận cho bệnh nhân khác.");
            }

            appt.Status = status;
            if (!string.IsNullOrWhiteSpace(notes))
                appt.TherapyNotes = notes;

            await _appointments.UpdateAsync(appt);
            await _appointments.SaveChangesAsync();
            return (true, "Cập nhật trạng thái thành công.");
        }

        public async Task DeleteAsync(int id)
        {
            await _appointments.DeleteAsync(id);
            await _appointments.SaveChangesAsync();
        }

        public Task<int> CountByStatusAsync(AppointmentStatus status)
            => _db.Appointments.CountAsync(a => a.Status == status);

        public async Task<decimal> TotalRevenueAsync()
            => await _db.Appointments
                .Include(a => a.Service)
                .Where(a => a.Status == AppointmentStatus.Completed)
                .SumAsync(a => (decimal?)a.Service.Price) ?? 0m;
    }
}
