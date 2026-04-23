using Microsoft.EntityFrameworkCore;
using Montra.DAL.Context;
using Montra.DAL.Entities;

namespace Montra.DAL.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _db;
        public AppointmentRepository(AppDbContext db) => _db = db;

        public async Task<List<Appointment>> GetAllAsync(string? patientName = null, int? therapistId = null, AppointmentStatus? status = null, DateTime? date = null)
        {
            var q = _db.Appointments
                .Include(a => a.Therapist)
                .Include(a => a.Service)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(patientName))
                q = q.Where(a => a.PatientName.Contains(patientName));

            if (therapistId.HasValue)
                q = q.Where(a => a.TherapistId == therapistId.Value);

            if (status.HasValue)
                q = q.Where(a => a.Status == status.Value);

            if (date.HasValue)
                q = q.Where(a => a.AppointmentDate.Date == date.Value.Date);

            return await q.OrderByDescending(a => a.AppointmentDate).ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int id)
            => await _db.Appointments
                .Include(a => a.Therapist)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<List<Appointment>> GetByTherapistIdAsync(int therapistId)
            => await _db.Appointments
                .Include(a => a.Service)
                .Where(a => a.TherapistId == therapistId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

        public async Task<List<Appointment>> GetTodayByTherapistIdAsync(int therapistId)
        {
            var today = DateTime.Today;
            return await _db.Appointments
                .Include(a => a.Service)
                .Where(a => a.TherapistId == therapistId && a.AppointmentDate.Date == today)
                .OrderBy(a => a.TimeSlot)
                .ToListAsync();
        }

        public async Task<bool> HasConflictAsync(int therapistId, DateTime date, string timeSlot, int? excludeId = null)
        {
            var q = _db.Appointments.Where(a =>
                a.TherapistId == therapistId &&
                a.AppointmentDate.Date == date.Date &&
                a.TimeSlot == timeSlot &&
                a.Status != AppointmentStatus.Cancelled);

            if (excludeId.HasValue)
                q = q.Where(a => a.Id != excludeId.Value);

            return await q.AnyAsync();
        }

        public async Task AddAsync(Appointment appointment) => await _db.Appointments.AddAsync(appointment);
        public Task UpdateAsync(Appointment appointment)    { _db.Appointments.Update(appointment); return Task.CompletedTask; }

        public async Task DeleteAsync(int id)
        {
            var a = await _db.Appointments.FindAsync(id);
            if (a != null) _db.Appointments.Remove(a);
        }

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
    }
}
