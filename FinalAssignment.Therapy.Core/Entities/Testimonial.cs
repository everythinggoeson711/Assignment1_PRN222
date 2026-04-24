using System.ComponentModel.DataAnnotations;

namespace FinalAssignment.Therapy.Core.Entities;

public class Testimonial
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsApproved { get; set; } = true;
}
