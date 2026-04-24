using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Enums;
using FinalAssignment.Therapy.Core.Interfaces;
using FinalAssignment.Therapy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinalAssignment.Therapy.Infrastructure.Repositories;

public class AppointmentRepository(TherapyDbContext dbContext) : IAppointmentRepository
{
    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
        => await dbContext.Appointments.AddAsync(appointment, cancellationToken);

    public Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        dbContext.Appointments.Update(appointment);
        return Task.CompletedTask;
    }

    public async Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await dbContext.Appointments
            .Include(appointment => appointment.TherapistProfile)
            .Include(appointment => appointment.TherapyService)
            .FirstOrDefaultAsync(appointment => appointment.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
        => await dbContext.Appointments
            .Include(appointment => appointment.TherapistProfile)
            .Include(appointment => appointment.TherapyService)
            .OrderByDescending(appointment => appointment.CreatedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetForTherapistAsync(int therapistProfileId, CancellationToken cancellationToken = default)
        => await dbContext.Appointments
            .Include(appointment => appointment.TherapyService)
            .Where(appointment => appointment.TherapistProfileId == therapistProfileId)
            .OrderByDescending(appointment => appointment.AppointmentStartUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetPendingPaymentOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
        => await dbContext.Appointments
            .Include(appointment => appointment.TherapistProfile)
            .Include(appointment => appointment.TherapyService)
            .Where(appointment => appointment.Status == AppointmentStatus.PendingPayment
                && appointment.PaymentStatus == PaymentStatus.Pending
                && appointment.CreatedAtUtc <= cutoffUtc)
            .ToListAsync(cancellationToken);

    public async Task<int> CountByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default)
        => await dbContext.Appointments.CountAsync(appointment => appointment.Status == status, cancellationToken);

    public async Task<decimal> SumConfirmedRevenueAsync(CancellationToken cancellationToken = default)
        => await dbContext.Appointments
            .Where(appointment => appointment.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(appointment => (decimal?)appointment.PriceSnapshot, cancellationToken) ?? 0m;

    public async Task<bool> HasConflictAsync(int therapistProfileId, DateTime appointmentStartUtc, CancellationToken cancellationToken = default)
        => await dbContext.Appointments.AnyAsync(
            appointment => appointment.TherapistProfileId == therapistProfileId
                && appointment.AppointmentStartUtc == appointmentStartUtc
                && appointment.Status != AppointmentStatus.Cancelled
                && appointment.Status != AppointmentStatus.Expired,
            cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
        => await dbContext.Appointments
            .Include(a => a.TherapistProfile)
            .Include(a => a.TherapyService)
            .OrderByDescending(a => a.AppointmentStartUtc)
            .ToListAsync(cancellationToken);

    public async Task<Appointment?> GetByTrackingCodeAsync(string trackingCode, CancellationToken cancellationToken = default)
        => await dbContext.Appointments
            .FirstOrDefaultAsync(a => a.TrackingCode == trackingCode, cancellationToken);

    public async Task<bool> HasConflictInRangeAsync(int therapistId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken)
        => await dbContext.Appointments
            .Include(a => a.TherapyService)
            .AnyAsync(
                a => a.TherapistProfileId == therapistId
                     && a.Status != AppointmentStatus.Cancelled
                     && a.Status != AppointmentStatus.Expired
                     && a.AppointmentStartUtc < endUtc
                     && a.AppointmentStartUtc.AddMinutes(a.TherapyService.DurationMinutes) > startUtc,
                cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetPendingPaymentPastDueAsync(DateTime threshold, CancellationToken cancellationToken = default)
        => await dbContext.Appointments.Where(a => a.Status == AppointmentStatus.PendingPayment && a.CreatedAtUtc < threshold).ToListAsync(cancellationToken);


    public async Task RemoveAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        dbContext.Appointments.Remove(appointment);
        await Task.CompletedTask;
    }
}