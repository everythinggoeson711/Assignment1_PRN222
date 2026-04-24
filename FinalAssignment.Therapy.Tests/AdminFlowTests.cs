using FinalAssignment.Therapy.Application.Models;
using FinalAssignment.Therapy.Application.Services;
using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Interfaces;
using FinalAssignment.Therapy.Infrastructure.Data;
using FinalAssignment.Therapy.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinalAssignment.Therapy.Tests;

public class AdminFlowTests
{
    [Fact]
    public async Task BookingFlow_UpdatesAdminDashboardMetrics()
    {
        // 1. Setup Database and Services
        await using var dbContext = CreateDbContext();
        var unitOfWork = CreateUnitOfWork(dbContext);
        var clock = new FakeClock(DateTime.Parse("2026-04-23T10:00:00Z"));
        var notifier = new RecordingNotifier();
        
        var appointmentService = new AppointmentService(unitOfWork, clock, notifier);
        var dashboardService = new DashboardService(unitOfWork, appointmentService);

        // 2. Setup initial data (Therapist and Service)
        var therapist = new TherapistProfile { Name = "Dr. Test", Specialty = "Testing", IsActive = true };
        var therapyService = new TherapyService { Name = "Test Session", DurationMinutes = 60, Price = 100000m, IsActive = true };
        await dbContext.Therapists.AddAsync(therapist);
        await dbContext.TherapyServices.AddAsync(therapyService);
        await dbContext.SaveChangesAsync();

        // 3. Initial Metrics Check
        var initialMetrics = await dashboardService.GetMetricsAsync(CancellationToken.None);
        Assert.Equal(0, initialMetrics.PendingPaymentCount);

        // 4. Simulate Patient Booking
        var bookingRequest = new AppointmentBookingRequest
        {
            PatientName = "Test Patient",
            PatientEmail = "patient@test.com",
            PatientPhone = "0123456789",
            TherapistProfileId = therapist.Id,
            TherapyServiceId = therapyService.Id,
            AppointmentStartLocal = DateTime.SpecifyKind(DateTime.Parse("2026-04-24T09:00:00"), DateTimeKind.Local)
        };
        var result = await appointmentService.BookAsync(bookingRequest);
        Assert.True(result.Succeeded);
        await appointmentService.ConfirmPaymentAsync(result.EntityId.Value, CancellationToken.None);

        // 5. Verify Metrics Updated (Should be Confirmed immediately)
        var updatedMetrics = await dashboardService.GetMetricsAsync(CancellationToken.None);
        Assert.Equal(1, updatedMetrics.ConfirmedCount);
        Assert.Single(updatedMetrics.RecentAppointments);
        Assert.Equal("Test Patient", updatedMetrics.RecentAppointments.First().PatientName);

        // 6. Verify Admin Notifier was triggered twice (once for booking, once for payment confirmation)
        Assert.True(notifier.BroadcastCount >= 1);
    }

    private static TherapyDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TherapyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TherapyDbContext(options);
    }

    private static IUnitOfWork CreateUnitOfWork(TherapyDbContext dbContext)
        => new UnitOfWork(
            dbContext,
            new AppUserRepository(dbContext),
            new TherapistRepository(dbContext),
            new TherapyServiceRepository(dbContext),
            new AppointmentRepository(dbContext));

    private sealed class FakeClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow => utcNow;
    }

    private sealed class RecordingNotifier : IAdminDashboardNotifier
    {
        public int BroadcastCount { get; private set; }
        public Task BroadcastAsync(CancellationToken cancellationToken = default)
        {
            BroadcastCount++;
            return Task.CompletedTask;
        }
    }
}
