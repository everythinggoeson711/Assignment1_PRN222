// Add this method to ChatService to clear test sessions
public async Task ClearTestSessionsAsync()
{
    var allSessions = await _context.ChatSessions.ToListAsync();
    _context.ChatSessions.RemoveRange(allSessions);
    await _context.SaveChangesAsync();
}