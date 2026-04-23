namespace FinalAssignment.Therapy.Application.Models;

public class CreateTherapyServiceRequest
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;
}