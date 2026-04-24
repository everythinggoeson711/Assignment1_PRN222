using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Interfaces;
using FinalAssignment.Therapy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinalAssignment.Therapy.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly TherapyDbContext _context;

    public ChatService(TherapyDbContext context)
    {
        _context = context;
    }

    public async Task<ChatSession> CreateSessionAsync(string customerName, string? customerEmail, string connectionId)
    {
        var session = new ChatSession
        {
            SessionId = Guid.NewGuid().ToString(),
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CustomerConnectionId = connectionId,
            StartedAt = DateTime.UtcNow,
            IsActive = true,
            Status = ChatSessionStatus.WaitingForStaff
        };

        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<ChatSession?> GetSessionByIdAsync(string sessionId)
    {
        return await _context.ChatSessions
            .Include(cs => cs.Messages)
            .Include(cs => cs.AssignedStaff)
            .FirstOrDefaultAsync(cs => cs.SessionId == sessionId);
    }

    public async Task<List<ChatSession>> GetActiveSessions()
    {
        return await _context.ChatSessions
            .Include(cs => cs.AssignedStaff)
            .Where(cs => cs.IsActive)
            .OrderByDescending(cs => cs.StartedAt)
            .ToListAsync();
    }

    public async Task ClearAllSessionsAsync()
    {
        var allSessions = await _context.ChatSessions.Include(cs => cs.Messages).ToListAsync();
        _context.ChatSessions.RemoveRange(allSessions);
        await _context.SaveChangesAsync();
        Console.WriteLine($"Cleared {allSessions.Count} chat sessions from database");
    }

    public async Task<List<ChatSession>> GetUnassignedSessions()
    {
        return await _context.ChatSessions
            .Where(cs => cs.IsActive && cs.AssignedStaffId == null)
            .OrderBy(cs => cs.StartedAt)
            .ToListAsync();
    }

    public async Task<bool> AssignSessionToStaffAsync(string sessionId, int staffId, string staffConnectionId)
    {
        var session = await _context.ChatSessions
            .FirstOrDefaultAsync(cs => cs.SessionId == sessionId && cs.IsActive);

        if (session == null) return false;

        session.AssignedStaffId = staffId;
        session.StaffConnectionId = staffConnectionId;
        session.Status = ChatSessionStatus.Active;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EndSessionAsync(string sessionId)
    {
        var session = await _context.ChatSessions
            .FirstOrDefaultAsync(cs => cs.SessionId == sessionId);

        if (session == null) return false;

        session.IsActive = false;
        session.Status = ChatSessionStatus.Ended;
        session.EndedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddMessageAsync(string sessionId, string senderName, string message, bool isFromStaff)
    {
        var session = await _context.ChatSessions
            .FirstOrDefaultAsync(cs => cs.SessionId == sessionId);

        if (session == null) return false;

        var chatMessage = new ChatMessage
        {
            ChatSessionId = session.Id,
            SenderName = senderName,
            Message = message,
            IsFromStaff = isFromStaff,
            SentAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId)
    {
        var session = await _context.ChatSessions
            .FirstOrDefaultAsync(cs => cs.SessionId == sessionId);

        if (session == null) return new List<ChatMessage>();

        return await _context.ChatMessages
            .Where(cm => cm.ChatSessionId == session.Id)
            .OrderBy(cm => cm.SentAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateConnectionAsync(string sessionId, string connectionId, bool isStaff)
    {
        var session = await _context.ChatSessions
            .FirstOrDefaultAsync(cs => cs.SessionId == sessionId);

        if (session == null) return false;

        if (isStaff)
            session.StaffConnectionId = connectionId;
        else
            session.CustomerConnectionId = connectionId;

        await _context.SaveChangesAsync();
        return true;
    }
}