using System;
using System.Windows;
using System.Windows.Media;

namespace CybersecurityChatbot_Part2.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? ReminderDate { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string DisplayReminder
        {
            get
            {
                if (ReminderDate.HasValue)
                    return $"🔔 Reminder: {ReminderDate.Value:MMM dd, yyyy}";
                return "No reminder set";
            }
        }

        public string StatusIcon => Status == "completed" ? "✅" : "⏳";

        public TextDecorationCollection CompletedDecorations
        {
            get
            {
                if (Status == "completed")
                {
                    return new TextDecorationCollection { new TextDecoration() };
                }
                return null;
            }
        }

        public SolidColorBrush StatusColor
        {
            get
            {
                if (Status == "completed")
                    return new SolidColorBrush(Colors.Green);
                return new SolidColorBrush(Colors.Orange);
            }
        }

        public Visibility ShowCompletionDate
        {
            get
            {
                return CompletedAt.HasValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}