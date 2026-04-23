namespace FinalAssignment.Therapy.Application.Models;

public class AuthenticatedUser
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int? TherapistProfileId { get; set; }
}