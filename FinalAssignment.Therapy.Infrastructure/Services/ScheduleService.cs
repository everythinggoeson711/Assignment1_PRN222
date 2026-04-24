using FinalAssignment.Therapy.Core.Enums;
using FinalAssignment.Therapy.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalAssignment.Therapy.Infrastructure.Services;

public class ScheduleService : IScheduleService
{
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsTimeSlotAvailableAsync(int therapistId, DateTime startTime, int durationMinutes, CancellationToken cancellationToken = default)
    {
        var therapist = await _unitOfWork.Therapists.GetByIdAsync(therapistId, cancellationToken);
        if (therapist == null || !therapist.IsActive)
            return false;

        // Check if it's within working hours
        if (!IsWithinWorkingHours(therapist, startTime, durationMinutes))
            return false;

        // Check if it's a working day
        if (!IsWorkingDay(therapist, startTime))
            return false;

        // Check for existing appointments
        var endTime = startTime.AddMinutes(durationMinutes);
        var hasConflict = await _unitOfWork.Appointments.HasConflictInRangeAsync(therapistId, startTime, endTime, cancellationToken);
        
        return !hasConflict;
    }

    public async Task<List<DateTime>> GetAvailableSlotsAsync(int therapistId, DateTime date, int serviceDurationMinutes, CancellationToken cancellationToken = default)
    {
        var therapist = await _unitOfWork.Therapists.GetByIdAsync(therapistId, cancellationToken);
        if (therapist == null || !therapist.IsActive)
            return new List<DateTime>();

        if (!IsWorkingDay(therapist, date))
            return new List<DateTime>();

        var workStart = therapist.StartWorkTime ?? new TimeOnly(8, 0);
        var workEnd = therapist.EndWorkTime ?? new TimeOnly(17, 0);
        if (workEnd <= workStart)
            return new List<DateTime>();

        var slots = new List<DateTime>();
        var current = date.Date.Add(workStart.ToTimeSpan());
        var endOfWork = date.Date.Add(workEnd.ToTimeSpan());
        var stepMinutes = serviceDurationMinutes + therapist.BreakBetweenSlots;

        while (current.AddMinutes(serviceDurationMinutes) <= endOfWork)
        {
            if (current > DateTime.Now && await IsTimeSlotAvailableAsync(therapistId, current, serviceDurationMinutes, cancellationToken))
            {
                slots.Add(current);
            }
            current = current.AddMinutes(stepMinutes);
        }

        return slots;
    }

    public async Task<List<FinalAssignment.Therapy.Core.Models.TherapistScheduleDto>> GetDetailedScheduleAsync(int therapistId, DateTime date, CancellationToken cancellationToken = default)
    {
        var therapist = await _unitOfWork.Therapists.GetByIdAsync(therapistId, cancellationToken);
        if (therapist == null) return new List<FinalAssignment.Therapy.Core.Models.TherapistScheduleDto>();

        var appointments = await _unitOfWork.Appointments.GetForTherapistAsync(therapistId, cancellationToken);
        var dayAppointments = appointments
            .Where(a => a.AppointmentStartUtc.ToLocalTime().Date == date.Date && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Expired)
            .ToList();

        var workStart = therapist.StartWorkTime ?? new TimeOnly(8, 0);
        var workEnd = therapist.EndWorkTime ?? new TimeOnly(17, 0);
        
        var result = new List<FinalAssignment.Therapy.Core.Models.TherapistScheduleDto>();
        
        // Let's generate slots based on the smallest service duration or a fixed interval (e.g. 30 mins)
        // Or better, just show the booked slots and the available slots.
        
        // For simplicity in the UI, let's just return the list of appointments for that day as "booked slots"
        // and then we can also calculate "available" slots if needed.
        
        foreach (var appt in dayAppointments)
        {
            result.Add(new FinalAssignment.Therapy.Core.Models.TherapistScheduleDto
            {
                StartTime = appt.AppointmentStartUtc.ToLocalTime(),
                EndTime = appt.AppointmentStartUtc.ToLocalTime().AddMinutes(appt.TherapyService.DurationMinutes),
                IsBooked = true,
                PatientName = appt.PatientName,
                ServiceName = appt.TherapyService.Name,
                Status = appt.Status
            });
        }
        
        return result.OrderBy(r => r.StartTime).ToList();
    }

    private bool IsWithinWorkingHours(FinalAssignment.Therapy.Core.Entities.TherapistProfile therapist, DateTime startTime, int durationMinutes)
    {
        var workStart = therapist.StartWorkTime ?? new TimeOnly(8, 0);
        var workEnd = therapist.EndWorkTime ?? new TimeOnly(17, 0);
        
        var timeOfDay = TimeOnly.FromDateTime(startTime);
        var endTimeOfDay = TimeOnly.FromDateTime(startTime.AddMinutes(durationMinutes));

        return timeOfDay >= workStart && endTimeOfDay <= workEnd;
    }

    private bool IsWorkingDay(FinalAssignment.Therapy.Core.Entities.TherapistProfile therapist, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(therapist.WorkingDays))
        {
            return true;
        }

        var dayOfWeek = (int)date.DayOfWeek;
        if (dayOfWeek == 0)
        {
            dayOfWeek = 7; // Sunday as 7
        }

        var allowedDays = therapist.WorkingDays
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .ToHashSet();

        return allowedDays.Contains(dayOfWeek);
    }
}