using FinalAssignment.Therapy.Application.Models;
using FinalAssignment.Therapy.Application.Services;
using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Interfaces;
using FinalAssignment.Therapy.Infrastructure.Data;
using FinalAssignment.Therapy.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinalAssignment.Therapy.Tests;

public class AppointmentWorkflowTests
{
    [Fact]
    public async Task BookAsync_CreatesPendingPaymentAppointment()
    {
        await using var dbContext = CreateDbContext();
        var therapist = new TherapistProfile { Name = "Therapist A", Specialty = "Anxiety", IsActive = true };
        var therapyService = new TherapyService { Name = "Session", DurationMinutes = 60, Price = 500000m, IsActive = true };

        await dbContext.Therapists.AddAsync(therapist);
        await dbContext.TherapyServices.AddAsync(therapyService);
        await dbContext.SaveChangesAsync();

        var notifier = new RecordingNotifier();
        var service = new AppointmentService(CreateUnitOfWork(dbContext), new FakeClock(DateTime.Parse("2026-04-23T10:00:00Z")), notifier);

        var result = await service.BookAsync(new AppointmentBookingRequest
        {
            PatientName = "Jamie Doe",
            PatientEmail = "jamie@example.com",
            PatientPhone = "0909999999",
            TherapistProfileId = therapist.Id,
            TherapyServiceId = therapyService.Id,
            AppointmentStartLocal = DateTime.SpecifyKind(DateTime.Parse("2026-04-24T09:00:00"), DateTimeKind.Local)
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.EntityId);

        var savedAppointment = await dbContext.Appointments.FirstAsync();
        Assert.Equal(therapyService.Price, savedAppointment.PriceSnapshot);
        Assert.Equal(FinalAssignment.Therapy.Core.Enums.AppointmentStatus.PendingPayment, savedAppointment.Status);
        Assert.Equal(FinalAssignment.Therapy.Core.Enums.PaymentStatus.Pending, savedAppointment.PaymentStatus);
        Assert.Equal(1, notifier.BroadcastCount);
    }

    [Fact]
    public async Task ExpirePendingAppointmentsAsync_ExpiresOldPendingRecords()
    {
        await using var dbContext = CreateDbContext();

        var therapist = new TherapistProfile { Name = "Therapist B", Specialty = "Burnout", IsActive = true };
        var therapyService = new TherapyService { Name = "Workshop", DurationMinutes = 90, Price = 350000m, IsActive = true };
        await dbContext.Therapists.AddAsync(therapist);
        await dbContext.TherapyServices.AddAsync(therapyService);
        await dbContext.SaveChangesAsync();

        await dbContext.Appointments.AddRangeAsync(
        [
            new Appointment
            {
                PatientName = "Old Pending",
                PatientEmail = "old@example.com",
                PatientPhone = "0900000001",
                TherapistProfileId = therapist.Id,
                TherapyServiceId = therapyService.Id,
                AppointmentStartUtc = DateTime.Parse("2026-04-20T08:00:00Z"),
                PriceSnapshot = 350000m,
                CreatedAtUtc = DateTime.Parse("2026-04-20T07:00:00Z"),
                UpdatedAtUtc = DateTime.Parse("2026-04-20T07:00:00Z")
            },
            new Appointment
            {
                PatientName = "Fresh Pending",
                PatientEmail = "fresh@example.com",
                PatientPhone = "0900000002",
                TherapistProfileId = therapist.Id,
                TherapyServiceId = therapyService.Id,
                AppointmentStartUtc = DateTime.Parse("2026-04-23T08:00:00Z"),
                PriceSnapshot = 350000m,
                CreatedAtUtc = DateTime.Parse("2026-04-23T06:00:00Z"),
                UpdatedAtUtc = DateTime.Parse("2026-04-23T06:00:00Z")
            }
        ]);
        await dbContext.SaveChangesAsync();

        var notifier = new RecordingNotifier();
        var processor = new PendingAppointmentProcessor(
            CreateUnitOfWork(dbContext),
            new FakeClock(DateTime.Parse("2026-04-23T12:00:00Z")),
            notifier);

        var expiredCount = await processor.ExpirePendingAppointmentsAsync(TimeSpan.FromHours(24));

        Assert.Equal(1, expiredCount);
        Assert.Equal(1, notifier.BroadcastCount);

        var expiredAppointment = await dbContext.Appointments.SingleAsync(appointment => appointment.PatientName == "Old Pending");
        var freshAppointment = await dbContext.Appointments.SingleAsync(appointment => appointment.PatientName == "Fresh Pending");

        Assert.Equal(FinalAssignment.Therapy.Core.Enums.AppointmentStatus.Expired, expiredAppointment.Status);
        Assert.Equal(FinalAssignment.Therapy.Core.Enums.PaymentStatus.Expired, expiredAppointment.PaymentStatus);
        Assert.Equal(FinalAssignment.Therapy.Core.Enums.AppointmentStatus.PendingPayment, freshAppointment.Status);
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