using System.Text.RegularExpressions;
using FinalAssignment.Therapy.Application.Models;
using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Enums;
using FinalAssignment.Therapy.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace FinalAssignment.Therapy.Application.Services;

public class BookingService(IUnitOfWork unitOfWork, IOptions<SePaySettings> sePaySettings) : IBookingService
{
    private readonly SePaySettings _settings = sePaySettings.Value;

    public async Task<BookingResult> CreateBookingAsync(AppointmentBookingRequest request, CancellationToken cancellationToken = default)
    {
        var service = await unitOfWork.TherapyServices.GetByIdAsync(request.TherapyServiceId, cancellationToken);
        if (service == null) return new BookingResult { Success = false, Message = "Dịch vụ không tồn tại." };

        var therapist = await unitOfWork.Therapists.GetByIdAsync(request.TherapistProfileId, cancellationToken);
        if (therapist == null) return new BookingResult { Success = false, Message = "Chuyên gia không tồn tại." };

        var trackingCode = $"CBK-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        var appointment = new Appointment
        {
            PatientName = request.PatientName,
            PatientEmail = request.PatientEmail,
            PatientPhone = request.PatientPhone,
            TherapistProfileId = request.TherapistProfileId,
            TherapyServiceId = request.TherapyServiceId,
            AppointmentStartUtc = request.AppointmentStartLocal.ToUniversalTime(),
            Notes = request.Notes,
            PriceSnapshot = service.Price,
            TrackingCode = trackingCode,
            Status = AppointmentStatus.PendingPayment,
            PaymentStatus = PaymentStatus.Pending
        };

        await unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var qrCodeUrl = $"https://img.vietqr.io/image/{_settings.BankId}-{_settings.BankAccountNumber}-compact.png?amount={Math.Round(service.Price, 0)}&addInfo={trackingCode}&accountName={Uri.EscapeDataString(_settings.BankAccountName)}";

        return new BookingResult
        {
            Success = true,
            AppointmentId = appointment.Id,
            TrackingCode = trackingCode,
            QrCodeUrl = qrCodeUrl,
            Amount = service.Price,
            Message = "Vui lòng quét mã QR để thanh toán."
        };
    }

    private static readonly Regex TrackingCodePattern = new(@"CBK-\d{6}-[A-Z0-9]{8}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task ProcessSePayWebhookAsync(SePayWebhookData data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(data.Content)) return;

        var match = TrackingCodePattern.Match(data.Content);
        if (!match.Success) return;

        var trackingCode = match.Value.ToUpperInvariant();

        var appointment = await unitOfWork.Appointments.GetByTrackingCodeAsync(trackingCode, cancellationToken);
        if (appointment == null || appointment.PaymentStatus == PaymentStatus.Paid) return;

        if (data.TransferAmount >= (long)appointment.PriceSnapshot)
        {
            appointment.PaymentStatus = PaymentStatus.Paid;
            appointment.Status = AppointmentStatus.Confirmed;
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
