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
            return ServiceResult.Failure("Therapist không tồn tại hoặc đã ngưng hoạt động.");
        }

        var therapyService = await unitOfWork.TherapyServices.GetByIdAsync(request.TherapyServiceId, cancellationToken);
        if (therapyService is null || !therapyService.IsActive)
        {
            return ServiceResult.Failure("Dịch vụ trị liệu không tồn tại.");
        }

        var appointmentStartUtc = request.AppointmentStartLocal.Kind == DateTimeKind.Utc
            ? request.AppointmentStartLocal
            : DateTime.SpecifyKind(request.AppointmentStartLocal, DateTimeKind.Local).ToUniversalTime();

        var hasConflict = await unitOfWork.Appointments.HasConflictAsync(request.TherapistProfileId, appointmentStartUtc, cancellationToken);
        if (hasConflict)
        {
            return ServiceResult.Failure("Khung giờ này đã được đặt. Vui lòng chọn thời gian khác.");
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

        return ServiceResult.Success("Đặt lịch thành công. Hệ thống đang chờ xác nhận thanh toán.", appointment.Id);
    }

    public async Task<ServiceResult> ConfirmPaymentAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await unitOfWork.Appointments.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return ServiceResult.Failure("Không tìm thấy lịch hẹn.");
        }

        appointment.PaymentStatus = PaymentStatus.Paid;
        appointment.Status = AppointmentStatus.Confirmed;
        appointment.UpdatedAtUtc = clock.UtcNow;

        await unitOfWork.Appointments.UpdateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await dashboardNotifier.BroadcastAsync(cancellationToken);

        return ServiceResult.Success("Lịch hẹn đã được xác nhận thanh toán.");
    }

    public async Task<ServiceResult> CompleteAsync(int appointmentId, int therapistProfileId, CancellationToken cancellationToken = default)
    {
        var appointment = await unitOfWork.Appointments.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return ServiceResult.Failure("Không tìm thấy lịch hẹn.");
        }

        if (appointment.TherapistProfileId != therapistProfileId)
        {
            return ServiceResult.Failure("Bạn không có quyền thao tác trên lịch hẹn này.");
        }

        appointment.Status = AppointmentStatus.Completed;
        appointment.UpdatedAtUtc = clock.UtcNow;

        await unitOfWork.Appointments.UpdateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await dashboardNotifier.BroadcastAsync(cancellationToken);

        return ServiceResult.Success("Đã hoàn tất buổi trị liệu.");
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
            PriceSnapshot = appointment.PriceSnapshot
        };
}