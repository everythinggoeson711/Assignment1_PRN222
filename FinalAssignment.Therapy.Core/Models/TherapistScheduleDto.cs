using FinalAssignment.Therapy.Core.Enums;

namespace FinalAssignment.Therapy.Core.Models;

public class TherapistScheduleDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBooked { get; set; }
    public string? PatientName { get; set; }
    public string? ServiceName { get; set; }
    public AppointmentStatus? Status { get; set; }
}
