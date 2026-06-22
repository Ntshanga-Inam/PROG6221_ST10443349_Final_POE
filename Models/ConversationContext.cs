using System.Collections.Generic;

namespace CybersecurityChatbot_Part2.Models
{
    public class ConversationContext
    {
        public string CurrentTopic { get; set; }
        public List<string> RecentMessages { get; set; }
        public int MessageCount { get; set; }

        public ConversationContext()
        {
            RecentMessages = new List<string>();
            MessageCount = 0;
        }

        public void AddMessage(string message)
        {
            RecentMessages.Add(message);
            if (RecentMessages.Count > 10)
                RecentMessages.RemoveAt(0);
            MessageCount++;
        }
    }
}