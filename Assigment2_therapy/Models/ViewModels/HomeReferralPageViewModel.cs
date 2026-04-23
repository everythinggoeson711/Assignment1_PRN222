using Montra.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace Assigment2_therapy.Models.ViewModels
{
    public class HomeReferralPageViewModel
    {
        [Required(ErrorMessage = "Vui long nhap ten participant.")]
        [Display(Name = "Participant full name")]
        public string ParticipantName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "NDIS number")]
        public string NdisNumber { get; set; } = string.Empty;

        [Display(Name = "Contact phone")]
        public string ParticipantPhone { get; set; } = string.Empty;

        [Display(Name = "Participant email")]
        [EmailAddress(ErrorMessage = "Email khong hop le.")]
        public string ParticipantEmail { get; set; } = string.Empty;

        [Display(Name = "Home address")]
        public string HomeAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long nhap ten nguoi gioi thieu.")]
        [Display(Name = "Referrer name")]
        public string ReferrerName { get; set; } = string.Empty;

        [Display(Name = "Organisation")]
        public string Organisation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long nhap email nguoi gioi thieu.")]
        [EmailAddress(ErrorMessage = "Email khong hop le.")]
        [Display(Name = "Referrer email")]
        public string ReferrerEmail { get; set; } = string.Empty;

        [Display(Name = "Referrer phone")]
        public string ReferrerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long chon vai tro nguoi gioi thieu.")]
        [Display(Name = "Referrer role")]
        public string ReferrerRole { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long chon dich vu.")]
        [Display(Name = "Service")]
        public int ServiceId { get; set; }

        [Display(Name = "Preferred clinician")]
        public int? PreferredTherapistId { get; set; }

        [Required(ErrorMessage = "Vui long chon ly do chinh.")]
        [Display(Name = "Primary reason")]
        public string PrimaryReason { get; set; } = string.Empty;

        [Display(Name = "Reason detail")]
        public string MainReason { get; set; } = string.Empty;

        public List<string> KeyConcerns { get; set; } = new();

        public List<string> DesiredOutcomes { get; set; } = new();

        [Required(ErrorMessage = "Vui long chon muc do uu tien.")]
        [Display(Name = "Urgency level")]
        public string UrgencyLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long chon cach lien he uu tien.")]
        [Display(Name = "Preferred contact method")]
        public string PreferredContactMethod { get; set; } = string.Empty;

        [Display(Name = "Additional notes")]
        public string AdditionalNotes { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true", ErrorMessage = "Ban can dong y voi privacy consent.")]
        public bool PrivacyConsent { get; set; }

        public List<TherapyService> Services { get; set; } = new();
        public List<Therapist> Therapists { get; set; } = new();
        public int ActiveServiceCount { get; set; }
        public int ActiveTherapistCount { get; set; }
        public int ReferralQueueCount { get; set; }
        public int ConfirmedAppointmentCount { get; set; }
        public string TrackingCodeInput { get; set; } = string.Empty;
        public string TrackingLookupError { get; set; } = string.Empty;
        public PublicTrackingLookupViewModel? TrackingLookup { get; set; }

        public List<string> ReferrerRoles { get; } = new()
        {
            "Support Coordinator",
            "Family / Carer",
            "Allied Health",
            "Case Manager",
            "General Practitioner",
            "Other"
        };

        public List<string> PrimaryReasons { get; } = new()
        {
            "Initial FCA",
            "Plan Reassessment",
            "Hospital Discharge",
            "Assistive Technology",
            "Home Modifications",
            "Complex Support Review"
        };

        public List<string> ConcernAreaOptions { get; } = new()
        {
            "Mobility",
            "Daily care",
            "Falls risk",
            "Behaviour support",
            "Cognitive functioning",
            "Community access",
            "Housing setup",
            "Carer strain"
        };

        public List<string> OutcomeOptions { get; } = new()
        {
            "Funding justification",
            "Therapy roadmap",
            "Assistive technology",
            "Home modifications",
            "Support hours review",
            "Discharge planning"
        };

        public List<string> UrgencyOptions { get; } = new()
        {
            "Routine",
            "Priority within 2 weeks",
            "Urgent within 72 hours"
        };

        public List<string> ContactMethodOptions { get; } = new()
        {
            "Email",
            "Phone",
            "Either"
        };
    }

    public class PublicTrackingLookupViewModel
    {
        public string TrackingCode { get; set; } = string.Empty;
        public string TrackingType { get; set; } = string.Empty;
        public string PrimaryStatus { get; set; } = string.Empty;
        public string SecondaryStatus { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string ContactLine { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string ClinicianLine { get; set; } = string.Empty;
        public string TimelineLine { get; set; } = string.Empty;
        public string SupportingLine { get; set; } = string.Empty;
        public string NotesLine { get; set; } = string.Empty;
    }
}