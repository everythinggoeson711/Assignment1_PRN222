using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Montra.DAL.Entities
{
    public enum AppointmentStatus
    {
        Pending = 0,
        Confirmed = 1,
        Completed = 2,
        Cancelled = 3
    }

    public class Appointment
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string PatientName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PatientPhone { get; set; } = string.Empty;

        [MaxLength(150)]
        public string PatientEmail { get; set; } = string.Empty;

        public int TherapistId { get; set; }

        [ForeignKey(nameof(TherapistId))]
        public Therapist Therapist { get; set; } = null!;

        public int ServiceId { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public TherapyService Service { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }

        [Required, MaxLength(25)]
        public string TimeSlot { get; set; } = string.Empty;

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [MaxLength(2000)]
        public string? TherapyNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
