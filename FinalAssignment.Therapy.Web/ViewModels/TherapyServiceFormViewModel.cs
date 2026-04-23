using System.ComponentModel.DataAnnotations;

namespace FinalAssignment.Therapy.Web.ViewModels;

public class TherapyServiceFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 999999999)]
    public decimal Price { get; set; }

    [Range(15, 480)]
    public int DurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;
}