using System.ComponentModel.DataAnnotations;

namespace FinalAssignment.Therapy.Web.ViewModels;

public class TherapistFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Specialty { get; set; } = string.Empty;

    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Bio { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [Range(0, 50)]
    public int YearsOfExperience { get; set; }

    [StringLength(200)]
    public string? Education { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [DataType(DataType.Password)]
    public string? Password { get; set; }
}