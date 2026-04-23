using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Montra.DAL.Entities
{
    public class CenterServiceBooking
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string ParticipantName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string ParticipantEmail { get; set; } = string.Empty;

        [MaxLength(20)]
        public string ParticipantPhone { get; set; } = string.Empty;

        public int ServiceId { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public TherapyService Service { get; set; } = null!;

        public int TherapistId { get; set; }

        [ForeignKey(nameof(TherapistId))]
        public Therapist Therapist { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }

        [Required, MaxLength(25)]
        public string TimeSlot { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        [Required, MaxLength(10)]
        public string Currency { get; set; } = "vnd";

        [Required, MaxLength(30)]
        public string PaymentStatus { get; set; } = "Pending";

        [Required, MaxLength(30)]
        public string BookingStatus { get; set; } = "PendingPayment";

        [Required, MaxLength(40)]
        public string TrackingCode { get; set; } = string.Empty;

        [MaxLength(120)]
        public string StripeSessionId { get; set; } = string.Empty;

        [MaxLength(120)]
        public string StripePaymentIntentId { get; set; } = string.Empty;

        public DateTime? ExpiresAt { get; set; }

        public int? AppointmentId { get; set; }

        [ForeignKey(nameof(AppointmentId))]
        public Appointment? Appointment { get; set; }

        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}