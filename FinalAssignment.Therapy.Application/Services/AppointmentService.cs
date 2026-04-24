using FinalAssignment.Therapy.Application.Models;
using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Enums;
using FinalAssignment.Therapy.Core.Interfaces;

namespace FinalAssignment.Therapy.Application.Services;

public class AppointmentService(
    IUnitOfWork unitOfWork,
    IClock clock,
    IAdminDashboardNotifier dashboardNotifier) : IAppointmentService
{
    public async Task<ServiceResult> BookAsync(AppointmentBookingRequest request, CancellationToken cancellationToken = default)
    {
        var therapist = await unitOfWork.Therapists.GetByIdAsync(request.TherapistProfileId, cancellationToken);
        if (therapist is null || !therapist.IsActive)
        {
            return ServiceResult.Failure("Therapist not found or inactive.");
        }

        var therapyService = await unitOfWork.TherapyServices.GetByIdAsync(request.TherapyServiceId, cancellationToken);
        if (therapyService is null || !therapyService.IsActive)
        {
            return ServiceResult.Failure("Therapy service not found.");
        }

        var appointmentStartUtc = request.AppointmentStartLocal.Kind == DateTimeKind.Utc
            ? request.AppointmentStartLocal
            : DateTime.SpecifyKind(request.AppointmentStartLocal, DateTimeKind.Local).ToUniversalTime();

        if (appointmentStartUtc < clock.UtcNow)
        {
            return ServiceResult.Failure("Cannot book an appointment in the past.");
        }

        var hasConflict = await unitOfWork.Appointments.HasConflictAsync(request.TherapistProfileId, appointmentStartUtc, cancellationToken);
        if (hasConflict)
        {
            return ServiceResult.Failure("This time slot is already booked. Please choose another time.");
        }

        var appointment = new Appointment
        {
            PatientName = request.PatientName.Trim(),
            PatientEmail = request.PatientEmail.Trim(),
            PatientPhone = request.PatientPhone.Trim(),
            TherapistProfileId = request.TherapistProfileId,
            TherapyServiceId = request.TherapyServiceId,
            AppointmentStartUtc = appointmentStartUtc,
            Notes = request.Notes?.Trim(),
            Status = AppointmentStatus.PendingPayment,
            PaymentStatus = PaymentStatus.Pending,
            PriceSnapshot = therapyService.Price,
            CreatedAtUtc = clock.UtcNow,
            UpdatedAtUtc = clock.UtcNow
        };

        await unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await dashboardNotifier.BroadcastAsync(cancellationToken);

        return ServiceResult.Success("Booking successful. Waiting for payment confirmation.", appointment.Id);
    }

    public async Task<ServiceResult> ConfirmPaymentAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await unitOfWork.Appointments.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return ServiceResult.Failure("Appointment not found.");
        }

        appointment.PaymentStatus = PaymentStatus.Paid;
        appointment.Status = AppointmentStatus.Confirmed;
        appointment.UpdatedAtUtc = clock.UtcNow;

        await unitOfWork.Appointments.UpdateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await dashboardNotifier.BroadcastAsync(cancellationToken);

        return ServiceResult.Success("Payment confirmed and appointment scheduled.");
    }

    public async Task<ServiceResult> CompleteAsync(int appointmentId, int therapistProfileId, CancellationToken cancellationToken = default)
    {
        var appointment = await unitOfWork.Appointments.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return ServiceResult.Failure("Appointment not found.");
        }

        if (appointment.TherapistProfileId != therapistProfileId)
        {
            return ServiceResult.Failure("You do not have permission to manage this appointment.");
        }

        appointment.Status = AppointmentStatus.Completed;
        appointment.UpdatedAtUtc = clock.UtcNow;

        await unitOfWork.Appointments.UpdateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await dashboardNotifier.BroadcastAsync(cancellationToken);

        return ServiceResult.Success("Therapy session marked as completed.");
    }

    public async Task<IReadOnlyList<AppointmentSummaryDto>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        var appointments = await unitOfWork.Appointments.GetRecentAsync(count, cancellationToken);
        return appointments.Select(MapToSummary).ToList();
    }

    public async Task<IReadOnlyList<AppointmentSummaryDto>> GetForTherapistAsync(int therapistProfileId, CancellationToken cancellationToken = default)
    {
        var appointments = await unitOfWork.Appointments.GetForTherapistAsync(therapistProfileId, cancellationToken);
        return appointments.Select(MapToSummary).ToList();
    }

    public async Task<AppointmentSummaryDto?> GetAppointmentDetailsAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await unitOfWork.Appointments.GetByIdAsync(appointmentId, cancellationToken);
        return appointment is null ? null : MapToSummary(appointment);
    }

    public async Task<IReadOnlyList<AppointmentSummaryDto>> GetAllAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var appointments = await unitOfWork.Appointments.GetAllAsync(cancellationToken);
        return appointments.Select(MapToSummary).ToList();
    }

    private static AppointmentSummaryDto MapToSummary(Appointment appointment)
        => new()
        {
            Id = appointment.Id,
            PatientName = appointment.PatientName,
            PatientEmail = appointment.PatientEmail,
            TherapistName = appointment.TherapistProfile?.Name ?? string.Empty,
            ServiceName = appointment.TherapyService?.Name ?? string.Empty,
            AppointmentStartUtc = appointment.AppointmentStartUtc,
            Status = appointment.Status,
            PaymentStatus = appointment.PaymentStatus,
            PriceSnapshot = appointment.PriceSnapshot,
            TrackingCode = appointment.TrackingCode
        };
}