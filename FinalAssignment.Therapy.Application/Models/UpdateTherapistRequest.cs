namespace FinalAssignment.Therapy.Application.Models;

public class UpdateTherapistRequest
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Specialty { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}