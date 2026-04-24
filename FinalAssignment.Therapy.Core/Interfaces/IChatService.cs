using FinalAssignment.Therapy.Core.Entities;

namespace FinalAssignment.Therapy.Core.Interfaces;

public interface IChatService
{
    Task<ChatSession> CreateSessionAsync(string customerName, string? customerEmail, string connectionId);
    Task<ChatSession?> GetSessionByIdAsync(string sessionId);
    Task<List<ChatSession>> GetActiveSessions();
    Task<List<ChatSession>> GetUnassignedSessions();
    Task<bool> AssignSessionToStaffAsync(string sessionId, int staffId, string staffConnectionId);
    Task<bool> EndSessionAsync(string sessionId);
    Task<bool> AddMessageAsync(string sessionId, string senderName, string message, bool isFromStaff);
    Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId);
    Task<bool> UpdateConnectionAsync(string sessionId, string connectionId, bool isStaff);
    Task ClearAllSessionsAsync();
}