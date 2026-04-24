using FinalAssignment.Therapy.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FinalAssignment.Therapy.Web.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task JoinCustomerSupport(string customerName, string? customerEmail = null)
    {
        Console.WriteLine($"JoinCustomerSupport called: customerName={customerName}, connectionId={Context.ConnectionId}");
        
        var session = await _chatService.CreateSessionAsync(customerName, customerEmail, Context.ConnectionId);
        Console.WriteLine($"Session created: sessionId={session.SessionId}");
        
        var groupName = $"session_{session.SessionId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        Console.WriteLine($"Customer added to group: {groupName}");
        
        // Notify all admin/staff about new session
        await Clients.Group("AdminStaff").SendAsync("NewChatSession", new
        {
            sessionId = session.SessionId,
            customerName = session.CustomerName,
            startedAt = session.StartedAt
        });
        Console.WriteLine("Notified AdminStaff about new session");

        await Clients.Caller.SendAsync("SessionCreated", new
        {
            sessionId = session.SessionId,
            message = "Đã kết nối! Đang chờ nhân viên hỗ trợ..."
        });
        Console.WriteLine("SessionCreated event sent to customer");
        
        // Test group membership
        await Clients.Group(groupName).SendAsync("groupTest", "Customer joined the group");
    }

    public async Task JoinAsStaff()
    {
        Console.WriteLine($"JoinAsStaff called by: {Context.User?.Identity?.Name}");
        Console.WriteLine($"IsAuthenticated: {Context.User?.Identity?.IsAuthenticated}");
        Console.WriteLine($"Claims: {string.Join(", ", Context.User?.Claims?.Select(c => $"{c.Type}={c.Value}") ?? new string[0])}");
        
        // For now, allow anyone to join as staff for testing
        // TODO: Re-enable proper role checking later
        // if (Context.User?.IsInRole("Admin") == true || Context.User?.IsInRole("Therapist") == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminStaff");
            
            // Send list of unassigned sessions
            var unassignedSessions = await _chatService.GetUnassignedSessions();
            await Clients.Caller.SendAsync("UnassignedSessions", unassignedSessions.Select(s => new
            {
                sessionId = s.SessionId,
                customerName = s.CustomerName,
                startedAt = s.StartedAt
            }));
            
            Console.WriteLine($"Staff joined successfully, {unassignedSessions.Count()} unassigned sessions");
        }
    }

    public async Task AssignToSession(string sessionId)
    {
        Console.WriteLine($"AssignToSession called for sessionId: {sessionId} by user: {Context.User?.Identity?.Name}");
        
        // For testing, use a default user ID if not available
        // TODO: Re-enable proper authentication later
        // if (Context.User?.IsInRole("Admin") != true && Context.User?.IsInRole("Therapist") != true)
        //     return;

        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        int userId = 1; // Default admin user for testing
        
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var parsedUserId))
        {
            userId = parsedUserId;
        }
        
        Console.WriteLine($"Using userId: {userId}");

        var assigned = await _chatService.AssignSessionToStaffAsync(sessionId, userId, Context.ConnectionId);
        if (assigned)
        {
            var groupName = $"session_{sessionId}";
            
            // Force add to group multiple times to ensure it works
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Task.Delay(100); // Small delay
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"Staff DOUBLE-ADDED to group: {groupName}");
            
            // Test if staff is in group
            await Clients.Group(groupName).SendAsync("groupTest", "Staff assigned and joined the group");
            
            // Notify customer that staff joined
            await Clients.Group(groupName).SendAsync("StaffJoined", new
            {
                staffName = Context.User?.Identity?.Name ?? "Support Staff",
                message = "Nhân viên hỗ trợ đã tham gia cuộc trò chuyện"
            });
            Console.WriteLine("StaffJoined event sent to session group");
            
            // Remove from unassigned list for all staff
            await Clients.Group("AdminStaff").SendAsync("SessionAssigned", sessionId);
            Console.WriteLine($"Session {sessionId} assigned successfully");
        }
        else
        {
            Console.WriteLine($"Failed to assign session {sessionId}");
        }
    }

    public async Task SendMessageToSession(string sessionId, string message)
    {
        Console.WriteLine($"SendMessageToSession called: sessionId={sessionId}, message='{message}', user={Context.User?.Identity?.Name}");
        
        var session = await _chatService.GetSessionByIdAsync(sessionId);
        if (session == null) 
        {
            Console.WriteLine($"Session {sessionId} not found");
            return;
        }

        var isStaff = Context.User?.IsInRole("Admin") == true || Context.User?.IsInRole("Therapist") == true;
        var senderName = Context.User?.Identity?.Name ?? (isStaff ? "Support Staff" : "Customer");

        Console.WriteLine($"Sending message as: {senderName}, isStaff: {isStaff}");

        // Save message to database
        try 
        {
            await _chatService.AddMessageAsync(sessionId, senderName, message, isStaff);
            Console.WriteLine("Message saved to database successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving message to database: {ex.Message}");
        }

        // Send to all in this session
        var groupName = $"session_{sessionId}";
        Console.WriteLine($"Sending message to group: {groupName}");
        
        await Clients.Group(groupName).SendAsync("ReceiveMessage", new
        {
            sender = senderName,
            message = message,
            isFromStaff = isStaff,
            timestamp = DateTime.Now.ToString("HH:mm")
        });
        
        Console.WriteLine($"Message sent to group {groupName} successfully");
    }

    public async Task JoinSessionGroup(string sessionId)
    {
        Console.WriteLine($"JoinSessionGroup called for sessionId: {sessionId}, connectionId: {Context.ConnectionId}");
        
        var groupName = $"session_{sessionId}";
        
        // Force join multiple times to ensure it works
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Task.Delay(50);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Task.Delay(50);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        Console.WriteLine($"Connection {Context.ConnectionId} TRIPLE-ADDED to group: {groupName}");
        
        // Test group membership immediately
        await Clients.Group(groupName).SendAsync("groupTest", $"FORCE joined session group: {sessionId}");
        
        // Also send to caller to confirm
        await Clients.Caller.SendAsync("groupTest", $"You joined group {sessionId}");
    }

    public async Task EndSession(string sessionId)
    {
        var ended = await _chatService.EndSessionAsync(sessionId);
        if (ended)
        {
            await Clients.Group($"session_{sessionId}").SendAsync("SessionEnded", "Cuộc trò chuyện đã kết thúc");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Handle disconnection - could implement auto-reassignment logic here
        await base.OnDisconnectedAsync(exception);
    }
}
