using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalAssignment.Therapy.Core.Entities;

public class TherapistSchedule
{
    public int Id { get; set; }
    
    public int TherapistProfileId { get; set; }
    
    [ForeignKey(nameof(TherapistProfileId))]
    public TherapistProfile TherapistProfile { get; set; } = null!;
    
    public DateTime Date { get; set; } // Date only
    
    public TimeOnly StartTime { get; set; }
    
    public TimeOnly EndTime { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    [MaxLength(500)]
    public string? Notes { get; set; } // "Nghỉ phép", "Họp", etc.
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}