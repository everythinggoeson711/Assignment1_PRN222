using System.ComponentModel.DataAnnotations;

namespace FinalAssignment.Therapy.Core.Entities;

public class TherapistProfile
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;

    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Bio { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public TimeOnly? StartWorkTime { get; set; } = new TimeOnly(8, 0);

    public TimeOnly? EndWorkTime { get; set; } = new TimeOnly(17, 0);

    [MaxLength(20)]
    public string? WorkingDays { get; set; } = "1,2,3,4,5"; // Mon-Fri (1=Monday, 7=Sunday)

    public int SlotDurationMinutes { get; set; } = 60;

    public int BreakBetweenSlots { get; set; } = 15;

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public int YearsOfExperience { get; set; } = 0;

    [MaxLength(200)]
    public string? Education { get; set; }

    public double Rating { get; set; } = 5.0;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<TherapistSchedule> Schedules { get; set; } = new List<TherapistSchedule>();
}
