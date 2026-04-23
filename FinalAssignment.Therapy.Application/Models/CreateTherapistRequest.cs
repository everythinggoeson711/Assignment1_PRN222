namespace FinalAssignment.Therapy.Application.Models;

public class CreateTherapistRequest
{
    public string Name { get; set; } = string.Empty;

    public string Specialty { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}