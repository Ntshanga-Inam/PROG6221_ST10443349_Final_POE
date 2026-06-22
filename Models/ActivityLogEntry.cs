using System;

namespace CybersecurityChatbot_Part2.Models
{
    public class ActivityLogEntry
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        public string DisplayTime => Timestamp.ToString("HH:mm:ss");
        public string DisplayDate => Timestamp.ToString("MMM dd, yyyy");
    }
}