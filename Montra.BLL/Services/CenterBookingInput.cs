namespace Montra.BLL.Services
{
    public class CenterBookingInput
    {
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public string ParticipantPhone { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public int TherapistId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
