using FinalAssignment.Therapy.Core.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FinalAssignment.Therapy.Web.Hubs;

[Authorize(Roles = UserRoles.Admin)]
public class AdminDashboardHub : Hub
{
}