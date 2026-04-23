using System.ComponentModel.DataAnnotations;
using FinalAssignment.Therapy.Application.Models;

namespace FinalAssignment.Therapy.Web.ViewModels;

public class BookingFormViewModel
{
    [Required, StringLength(120)]
    public string PatientName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string PatientEmail { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string PatientPhone { get; set; } = string.Empty;

    [Required]
    public int TherapistProfileId { get; set; }

    [Required]
    public int TherapyServiceId { get; set; }

    [Required]
    public DateTime AppointmentStartLocal { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public IReadOnlyList<TherapistSummaryDto> AvailableTherapists { get; set; } = [];

    public IReadOnlyList<TherapyServiceSummaryDto> AvailableServices { get; set; } = [];
}