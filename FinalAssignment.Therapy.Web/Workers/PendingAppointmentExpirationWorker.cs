using FinalAssignment.Therapy.Application.Services;
using FinalAssignment.Therapy.Web.Options;
using Microsoft.Extensions.Options;

namespace FinalAssignment.Therapy.Web.Workers;

public class PendingAppointmentExpirationWorker(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<AppointmentCleanupOptions> options,
    ILogger<PendingAppointmentExpirationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(settings.ScanIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessAsync(settings, stoppingToken);
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task ProcessAsync(AppointmentCleanupOptions settings, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IPendingAppointmentProcessor>();
        var expiredCount = await processor.ExpirePendingAppointmentsAsync(TimeSpan.FromHours(settings.PendingPaymentLifetimeHours), cancellationToken);

        if (expiredCount > 0)
        {
            logger.LogInformation("Expired {ExpiredCount} pending appointments.", expiredCount);
        }
    }
}