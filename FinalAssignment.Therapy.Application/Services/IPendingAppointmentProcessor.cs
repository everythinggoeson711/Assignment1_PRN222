namespace FinalAssignment.Therapy.Application.Services;

public interface IPendingAppointmentProcessor
{
    Task<int> ExpirePendingAppointmentsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
}