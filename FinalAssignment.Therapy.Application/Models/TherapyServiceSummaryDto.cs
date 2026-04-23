namespace FinalAssignment.Therapy.Application.Models;

public class TherapyServiceSummaryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }
}