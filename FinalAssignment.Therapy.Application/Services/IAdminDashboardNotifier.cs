namespace FinalAssignment.Therapy.Application.Services;

public interface IAdminDashboardNotifier
{
    Task BroadcastAsync(CancellationToken cancellationToken = default);
}