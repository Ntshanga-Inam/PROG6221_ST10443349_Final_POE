using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using CybersecurityChatbot_Part2.Models;

namespace CybersecurityChatbot_Part2.Services
{
    public class DatabaseHelper
    {
        private string connectionString;

        public DatabaseHelper()
        {
            // Update with your MySQL credentials
            connectionString = "Server=localhost;Database=CybersecurityBot;Uid=root;Pwd=;";
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // TASK 1: Task Management Methods

        public bool AddTask(string title, string description, DateTime? reminderDate)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string query = @"INSERT INTO user_tasks (title, description, reminder_date, status) 
                                    VALUES (@title, @description, @reminderDate, 'pending')";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@description", description);
                        cmd.Parameters.AddWithValue("@reminderDate", reminderDate ?? (object)DBNull.Value);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                return false;
            }
        }

        public List<TaskItem> GetAllTasks()
        {
            var tasks = new List<TaskItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM user_tasks ORDER BY status ASC, reminder_date ASC";
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new TaskItem
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Description = reader.GetString("description"),
                                ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date")) ? null : reader.GetDateTime("reminder_date"),
                                Status = reader.GetString("status"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                CompletedAt = reader.IsDBNull(reader.GetOrdinal("completed_at")) ? null : reader.GetDateTime("completed_at")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
            }
            return tasks;
        }

        public List<TaskItem> GetPendingTasks()
        {
            var tasks = GetAllTasks();
            return tasks.FindAll(t => t.Status == "pending");
        }

        public List<TaskItem> GetCompletedTasks()
        {
            var tasks = GetAllTasks();
            return tasks.FindAll(t => t.Status == "completed");
        }

        public bool MarkTaskAsCompleted(int taskId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE user_tasks SET status = 'completed', completed_at = NOW() WHERE id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                return false;
            }
        }

        public bool DeleteTask(int taskId)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM user_tasks WHERE id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                return false;
            }
        }

        // Activity Log Methods - Enhanced for Task 4

        public bool LogActivity(string actionType, string description)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO activity_log (action_type, description) VALUES (@type, @desc)";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@type", actionType);
                        cmd.Parameters.AddWithValue("@desc", description);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                return false;
            }
        }

        public List<ActivityLogEntry> GetRecentActivities(int limit = 10)
        {
            var activities = new List<ActivityLogEntry>();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM activity_log ORDER BY timestamp DESC LIMIT @limit";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@limit", limit);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                activities.Add(new ActivityLogEntry
                                {
                                    Id = reader.GetInt32("id"),
                                    ActionType = reader.GetString("action_type"),
                                    Description = reader.GetString("description"),
                                    Timestamp = reader.GetDateTime("timestamp")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
            }
            return activities;
        }

        public int GetTotalActivitiesCount()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM activity_log";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                return 0;
            }
        }

        // NEW: Get activities with pagination for "Show More" feature
        public List<ActivityLogEntry> GetActivitiesPaginated(int page, int pageSize = 10)
        {
            var activities = new List<ActivityLogEntry>();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    int offset = (page - 1) * pageSize;
                    string query = "SELECT * FROM activity_log ORDER BY timestamp DESC LIMIT @offset, @pageSize";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@offset", offset);
                        cmd.Parameters.AddWithValue("@pageSize", pageSize);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                activities.Add(new ActivityLogEntry
                                {
                                    Id = reader.GetInt32("id"),
                                    ActionType = reader.GetString("action_type"),
                                    Description = reader.GetString("description"),
                                    Timestamp = reader.GetDateTime("timestamp")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
            }
            return activities;
        }

        public class ActivityLogEntry
        {
            public int Id { get; set; }
            public string ActionType { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }

            public string DisplayTime => Timestamp.ToString("HH:mm:ss");
            public string DisplayDate => Timestamp.ToString("MMM dd, yyyy");
            public string DisplayDateTime => Timestamp.ToString("MMM dd, yyyy HH:mm");
            public string Icon
            {
                get
                {
                    return ActionType switch
                    {
                        "Task Added" => "📝",
                        "Task Completed" => "✅",
                        "Task Deleted" => "🗑️",
                        "Task Creation" => "✏️",
                        "Reminder Set" => "🔔",
                        "Keyword Query" => "🔍",
                        "Quiz Started" => "🎮",
                        "Quiz Completed" => "🏆",
                        "User Registration" => "👤",
                        "NLP Interaction" => "🤖",
                        "NLP Query" => "🧠",
                        "NLP Intent" => "🎯",
                        _ => "•"
                    };
                }
            }
        }
    }
}