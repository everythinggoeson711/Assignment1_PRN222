using FinalAssignment.Therapy.Application.Services;
using FinalAssignment.Therapy.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FinalAssignment.Therapy.Web.Services;

public class SignalRAdminDashboardNotifier(IHubContext<AdminDashboardHub> hubContext) : IAdminDashboardNotifier
{
    public async Task BroadcastAsync(CancellationToken cancellationToken = default)
        => await hubContext.Clients.All.SendAsync("metricsChanged", cancellationToken);
}