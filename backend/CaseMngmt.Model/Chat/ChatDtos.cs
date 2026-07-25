namespace CaseMngmt.Models.Chat
{
    public class ChatMessageRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatHistoryTurn> History { get; set; } = new();
    }

    public class ChatHistoryTurn
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class ChatMessageResponse
    {
        public string Reply { get; set; } = string.Empty;
    }
}
