namespace FinalAssignment.Therapy.Application.Models;

public class AppointmentBookingRequest
{
    public string PatientName { get; set; } = string.Empty;

    public string PatientEmail { get; set; } = string.Empty;

    public string PatientPhone { get; set; } = string.Empty;

    public int TherapistProfileId { get; set; }

    public int TherapyServiceId { get; set; }

    public DateTime AppointmentStartLocal { get; set; }

    public string? Notes { get; set; }
}