using FinalAssignment.Therapy.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinalAssignment.Therapy.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITherapyLookupService, TherapyLookupService>();
        services.AddScoped<IAdminCatalogService, AdminCatalogService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPendingAppointmentProcessor, PendingAppointmentProcessor>();
        return services;
    }
}