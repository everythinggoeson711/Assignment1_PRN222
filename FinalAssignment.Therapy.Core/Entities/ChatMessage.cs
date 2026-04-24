using System.ComponentModel.DataAnnotations;

namespace FinalAssignment.Therapy.Core.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    
    public int ChatSessionId { get; set; }
    public ChatSession ChatSession { get; set; } = null!;
    
    [MaxLength(255)]
    public string SenderName { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    public bool IsFromStaff { get; set; }
    
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}