using FinalAssignment.Therapy.Core.Enums;

namespace FinalAssignment.Therapy.Application.Models;

public class AppointmentSummaryDto
{
    public int Id { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string PatientEmail { get; set; } = string.Empty;

    public string TherapistName { get; set; } = string.Empty;

    public string ServiceName { get; set; } = string.Empty;

    public DateTime AppointmentStartUtc { get; set; }

    public AppointmentStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public decimal PriceSnapshot { get; set; }
}