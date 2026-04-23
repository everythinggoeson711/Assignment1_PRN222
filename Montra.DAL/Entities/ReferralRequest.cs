using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Montra.DAL.Entities
{
    public class ReferralRequest
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string ParticipantName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(50)]
        public string NdisNumber { get; set; } = string.Empty;

        [MaxLength(20)]
        public string ParticipantPhone { get; set; } = string.Empty;

        [MaxLength(150)]
        public string ParticipantEmail { get; set; } = string.Empty;

        [MaxLength(250)]
        public string HomeAddress { get; set; } = string.Empty;

        [Required, MaxLength(120)]
        public string ReferrerName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Organisation { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string ReferrerEmail { get; set; } = string.Empty;

        [MaxLength(20)]
        public string ReferrerPhone { get; set; } = string.Empty;

        [MaxLength(80)]
        public string ReferrerRole { get; set; } = string.Empty;

        public int ServiceId { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public TherapyService Service { get; set; } = null!;

        public int? PreferredTherapistId { get; set; }

        [ForeignKey(nameof(PreferredTherapistId))]
        public Therapist? PreferredTherapist { get; set; }

        [MaxLength(200)]
        public string PrimaryReason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string MainReason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string KeyConcerns { get; set; } = string.Empty;

        [MaxLength(500)]
        public string DesiredOutcomes { get; set; } = string.Empty;

        [MaxLength(40)]
        public string UrgencyLevel { get; set; } = string.Empty;

        [MaxLength(40)]
        public string PreferredContactMethod { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string AdditionalNotes { get; set; } = string.Empty;

        public bool PrivacyConsent { get; set; }

        [Required, MaxLength(40)]
        public string TrackingCode { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Status { get; set; } = "New";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}