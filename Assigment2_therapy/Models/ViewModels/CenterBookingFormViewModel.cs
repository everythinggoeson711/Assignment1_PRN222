using System.ComponentModel.DataAnnotations;

namespace Assigment2_therapy.Models.ViewModels
{
    public class CenterBookingFormViewModel
    {
        [Required(ErrorMessage = "Vui long nhap ho ten.")]
        public string ParticipantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long nhap email.")]
        [EmailAddress(ErrorMessage = "Email khong hop le.")]
        public string ParticipantEmail { get; set; } = string.Empty;

        public string ParticipantPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long chon dich vu.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Vui long chon chuyen gia.")]
        public int TherapistId { get; set; }

        [Required(ErrorMessage = "Vui long chon ngay hen.")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Vui long chon khung gio.")]
        public string TimeSlot { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}