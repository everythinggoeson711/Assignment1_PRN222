namespace FinalAssignment.Therapy.Core.Interfaces;

public interface IScheduleService
{
    Task<bool> IsTimeSlotAvailableAsync(int therapistId, DateTime startTime, int durationMinutes, CancellationToken cancellationToken = default);
    Task<List<DateTime>> GetAvailableSlotsAsync(int therapistId, DateTime date, int serviceDurationMinutes, CancellationToken cancellationToken = default);
    Task<List<FinalAssignment.Therapy.Core.Models.TherapistScheduleDto>> GetDetailedScheduleAsync(int therapistId, DateTime date, CancellationToken cancellationToken = default);
}