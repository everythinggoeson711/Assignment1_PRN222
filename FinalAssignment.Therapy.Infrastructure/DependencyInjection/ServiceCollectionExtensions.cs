using FinalAssignment.Therapy.Core.Interfaces;
using FinalAssignment.Therapy.Infrastructure.Data;
using FinalAssignment.Therapy.Infrastructure.Repositories;
using FinalAssignment.Therapy.Infrastructure.Seed;
using FinalAssignment.Therapy.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinalAssignment.Therapy.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var databasePassword = configuration["Database:Password"]
            ?? configuration["FINAL_THERAPY_DB_PASSWORD"];

        if (connectionString.Contains("{DB_PASSWORD}", StringComparison.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new InvalidOperationException("Database password was not configured. Set Database:Password or FINAL_THERAPY_DB_PASSWORD.");
            }

            connectionString = connectionString.Replace("{DB_PASSWORD}", databasePassword, StringComparison.Ordinal);
        }

        services.AddDbContext<TherapyDbContext>(options => 
            options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<ITherapistRepository, TherapistRepository>();
        services.AddScoped<ITherapyServiceRepository, TherapyServiceRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IChatService, ChatService>();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();

        return services;
    }
}