using System.ComponentModel.DataAnnotations;

namespace FinalAssignment.Therapy.Core.Entities;

public class ChatSession
{
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    
    [MaxLength(255)]
    public string? CustomerName { get; set; }
    
    [MaxLength(255)]
    public string? CustomerEmail { get; set; }
    
    public string? CustomerConnectionId { get; set; }
    
    public int? AssignedStaffId { get; set; }
    public AppUser? AssignedStaff { get; set; }
    
    public string? StaffConnectionId { get; set; }
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? EndedAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public ChatSessionStatus Status { get; set; } = ChatSessionStatus.WaitingForStaff;
    
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

public enum ChatSessionStatus
{
    WaitingForStaff,
    Active,
    Ended
}