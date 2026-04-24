using System.ComponentModel.DataAnnotations;

namespace FinalAssignment.Therapy.Core.Entities;

public class TherapyService
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 999999)]
    public decimal Price { get; set; }

    [Range(15, 480)]
    public int DurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(50)]
    public string Category { get; set; } = "General";

    public string? DetailedDescription { get; set; }

    [MaxLength(500)]
    public string? Benefits { get; set; } // Comma separated list of benefits

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}