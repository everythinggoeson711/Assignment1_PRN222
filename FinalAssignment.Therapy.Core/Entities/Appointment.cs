using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinalAssignment.Therapy.Core.Enums;

namespace FinalAssignment.Therapy.Core.Entities;

public class Appointment
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string PatientName { get; set; } = string.Empty;

    [Required, MaxLength(180)]
    public string PatientEmail { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string PatientPhone { get; set; } = string.Empty;

    public int TherapistProfileId { get; set; }

    [ForeignKey(nameof(TherapistProfileId))]
    public TherapistProfile TherapistProfile { get; set; } = null!;

    public int TherapyServiceId { get; set; }

    [ForeignKey(nameof(TherapyServiceId))]
    public TherapyService TherapyService { get; set; } = null!;

    public DateTime AppointmentStartUtc { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.PendingPayment;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceSnapshot { get; set; }

    [MaxLength(30)]
    public string TrackingCode { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}