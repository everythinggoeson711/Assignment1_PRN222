namespace ChatClient.Models
{
    public class ChatMessage
    {
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsSystemMessage { get; set; }
        public bool IsOwnMessage { get; set; }

        public string AvatarInitial => string.IsNullOrEmpty(Username) ? "?" : Username.Substring(0, 1).ToUpper();

        public ChatMessage(string username, string content, bool isSystemMessage = false, bool isOwnMessage = false)
        {
            Username = username;
            Content = content;
            Timestamp = DateTime.Now;
            IsSystemMessage = isSystemMessage;
            IsOwnMessage = isOwnMessage;
        }

        public override string ToString()
        {
            if (IsSystemMessage)
            {
                return Content;
            }
            return $"[{Timestamp:HH:mm:ss}] {Content}";
        }
    }
}
