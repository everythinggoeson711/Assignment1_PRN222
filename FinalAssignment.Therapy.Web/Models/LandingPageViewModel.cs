using FinalAssignment.Therapy.Application.Models;

namespace FinalAssignment.Therapy.Web.Models;

public class LandingPageViewModel
{
    public IReadOnlyList<TherapistSummaryDto> Therapists { get; set; } = [];

    public IReadOnlyList<TherapyServiceSummaryDto> Services { get; set; } = [];
}