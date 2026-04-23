using System.Security.Claims;

namespace FinalAssignment.Therapy.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetTherapistProfileId(this ClaimsPrincipal principal)
    {
        var rawValue = principal.FindFirstValue("TherapistProfileId");
        return int.TryParse(rawValue, out var therapistProfileId) ? therapistProfileId : null;
    }
}