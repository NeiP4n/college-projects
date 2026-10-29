using System;

namespace WordAIAgent.Models
{
    public class ChatMessage
    {
        public string Role { get; set; }    // "user" | "assistant" | "system"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public ChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }
}
