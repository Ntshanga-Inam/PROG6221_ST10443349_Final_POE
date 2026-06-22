using System;
using System.Collections.Generic;
using System.Linq;
using CybersecurityChatbot_Part2.Models;

namespace CybersecurityChatbot_Part2.Services
{
    public class ChatbotEngine
    {
        private MemoryManager _memory;
        private SentimentAnalyzer _sentiment;
        private Random _random;

        // Keyword response dictionary
        private Dictionary<string, List<string>> _keywordResponses;

        // Random response pools
        private List<string> _phishingTips;
        private List<string> _passwordTips;
        private List<string> _generalTips;

        // Conversation context tracking
        private string _currentTopic;
        private List<string> _lastUserTopics;

        // NEW: Database Helper for Task Assistant and Activity Log
        private DatabaseHelper _dbHelper;

        // NEW: Task Assistant state tracking
        private bool _awaitingTaskReminder;
        private string _pendingTaskTitle;
        private string _pendingTaskDescription;

        public ChatbotEngine()
        {
            _memory = new MemoryManager();
            _sentiment = new SentimentAnalyzer();
            _random = new Random();
            _lastUserTopics = new List<string>();

            // NEW: Initialize Database Helper
            _dbHelper = new DatabaseHelper();

            // NEW: Initialize Task Assistant state
            _awaitingTaskReminder = false;
            _pendingTaskTitle = string.Empty;
            _pendingTaskDescription = string.Empty;

            InitializeKeywordResponses();
            InitializeRandomResponsePools();
        }

        private void InitializeKeywordResponses()
        {
            _keywordResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["password"] = new List<string>
                {
                    "Use strong passwords with at least 12 characters, mixing uppercase, lowercase, numbers, and symbols.",
                    "Never reuse passwords across different accounts. Use a password manager to keep track!",
                    "Enable two-factor authentication (2FA) whenever possible for an extra layer of security."
                },
                ["phish"] = new List<string>
                {
                    "Check email sender addresses carefully. Scammers often use addresses that look legitimate but have small typos.",
                    "Hover over links before clicking to see the actual URL. Don't enter personal info on suspicious sites.",
                    "Be wary of urgent language like 'Your account will be closed!' - scammers create false urgency."
                },
                ["scam"] = new List<string>
                {
                    "If it sounds too good to be true, it probably is a scam. Never send money to someone you haven't met in person.",
                    "Scammers impersonate banks and government agencies. Always call back on official numbers, not numbers they provide.",
                    "Don't share OTPs or PINs with anyone. Legitimate organisations will never ask for these."
                },
                ["privacy"] = new List<string>
                {
                    "Review app permissions regularly. Delete apps you don't use that have access to your data.",
                    "Use privacy-focused browser extensions and consider a VPN when using public Wi-Fi.",
                    "Be careful what you share on social media - oversharing helps scammers target you."
                },
                ["help"] = new List<string>
                {
                    "I can help with: passwords, phishing, scams, privacy, tasks, quiz, and safe browsing. Just ask!",
                    "Type a keyword like 'password', 'phishing', 'scam', or 'privacy' for tips, or ask for 'another tip'.",
                    "Try 'add task' to create a cybersecurity task, or 'quiz' to test your knowledge!"
                }
            };
        }

        private void InitializeRandomResponsePools()
        {
            _phishingTips = new List<string>
            {
                "🔍 Always check the sender's email address - scammers often use addresses like 'support@paypa1.com' instead of 'support@paypal.com'.",
                "📧 Never click links in unsolicited emails. Type the website address directly into your browser instead.",
                "⚠️ Look for spelling and grammar mistakes - legitimate companies rarely have errors in their communications.",
                "🔒 If an email asks for personal information, it's almost certainly a scam. Legitimate companies won't ask via email."
            };

            _passwordTips = new List<string>
            {
                "🔐 Use a passphrase instead of a password: 'PurpleDinosaurEatsPizza!' is stronger than 'P@ssw0rd'.",
                "🔄 Change your passwords every 3-6 months, especially for sensitive accounts like banking and email.",
                "🚫 Never use personal information (birthdays, pet names) in passwords - these are easily guessed or found on social media."
            };

            _generalTips = new List<string>
            {
                "💡 Always update your software and apps - updates often include important security patches.",
                "📱 Be careful what you download - only use official app stores and trusted websites.",
                "🌐 Use a different password for every account. If one gets hacked, the rest stay safe.",
                "📞 If someone calls claiming to be from your bank, hang up and call the bank's official number."
            };
        }

        // NEW: Main entry point with all features
        public string ProcessInputWithFeatures(string userInput)
        {
            // Store for conversation flow
            _lastUserTopics.Add(userInput);
            if (_lastUserTopics.Count > 5) _lastUserTopics.RemoveAt(0);

            // Check if user is asking for name (first interaction)
            if (!_memory.HasUserName() && !string.IsNullOrWhiteSpace(userInput))
            {
                _memory.SetUserName(userInput);
                _dbHelper.LogActivity("User Registration", $"New user: {userInput}");
                return $"Nice to meet you, {_memory.GetUserName()}! I'm your Cybersecurity Awareness Bot. What would you like to learn about today? You can ask about passwords, phishing, scams, privacy, tasks, or type 'quiz' to play a game!";
            }

            // NEW: Check for Quiz commands (Task 2)
            if (userInput.ToLower().Contains("quiz") || userInput.ToLower().Contains("play game") ||
                userInput.ToLower().Contains("test me") || userInput.ToLower().Contains("cybersecurity game") ||
                userInput.ToLower().Contains("start quiz"))
            {
                _dbHelper.LogActivity("Quiz Started", "User requested quiz");
                return "QUIZ_START";
            }

            // NEW: Check for Activity Log commands (Task 4 - Preview)
            if (userInput.ToLower().Contains("activity log") || userInput.ToLower().Contains("show log") ||
                userInput.ToLower().Contains("view log") || userInput.ToLower().Contains("recent activities"))
            {
                return HandleViewActivityLog();
            }

            // NEW: Check for Task Assistant commands first (Task 1)
            var taskResponse = ProcessTaskCommand(userInput);
            if (taskResponse != null)
                return taskResponse;

            // NEW: NLP Simulation
            var nlpResponse = ProcessNaturalLanguage(userInput);
            if (nlpResponse != null)
                return nlpResponse;

            // Handle follow-up requests
            if (IsFollowUpRequest(userInput))
            {
                return HandleFollowUp();
            }

            // Analyze sentiment
            var sentiment = _sentiment.AnalyzeSentiment(userInput);
            if (sentiment != Sentiment.Neutral)
            {
                return HandleSentimentResponse(sentiment, userInput);
            }

            // Check for keywords
            foreach (var keyword in _keywordResponses.Keys)
            {
                if (userInput.ToLower().Contains(keyword.ToLower()))
                {
                    _currentTopic = keyword;
                    _memory.SetFavoriteTopic(keyword);
                    var responses = _keywordResponses[keyword];
                    _dbHelper.LogActivity("Keyword Query", $"User asked about: {keyword}");
                    return responses[_random.Next(responses.Count)];
                }
            }

            // Check for "another tip" or similar
            if (userInput.ToLower().Contains("another") || userInput.ToLower().Contains("more"))
            {
                return GetRandomTip();
            }

            // Default response
            return "I'm not sure I understand. Can you try rephrasing? You can ask about passwords, phishing, scams, privacy, tasks, or type 'quiz' to play a game!";
        }

        // Keep original ProcessInput for backward compatibility
        public string ProcessInput(string userInput)
        {
            return ProcessInputWithFeatures(userInput);
        }

        // NEW: NLP Simulation method
        private string ProcessNaturalLanguage(string userInput)
        {
            string lowerInput = userInput.ToLower();

            // Advanced NLP pattern matching
            if (lowerInput.Contains("how to") || lowerInput.Contains("way to"))
            {
                if (lowerInput.Contains("protect") || lowerInput.Contains("secure"))
                    return "🔒 Great question! Start with strong passwords and enable 2FA on all accounts. Would you like specific steps? Just ask about 'password' or '2fa'!";
                if (lowerInput.Contains("spot") || lowerInput.Contains("identify") && lowerInput.Contains("phish"))
                    return "🎣 To spot phishing: check sender email addresses, hover over links, look for urgency, and never share personal info via email.";
                if (lowerInput.Contains("create") && lowerInput.Contains("password"))
                    return "🔐 To create a strong password: use at least 12 characters, mix uppercase/lowercase, add numbers and symbols, and avoid common words!";
            }

            if (lowerInput.Contains("what if") || lowerInput.Contains("what happens"))
            {
                if (lowerInput.Contains("click") && lowerInput.Contains("link"))
                    return "⚠️ If you accidentally click a suspicious link: disconnect from internet immediately, run antivirus scan, and change passwords for important accounts.";
                if (lowerInput.Contains("scam"))
                    return "📞 If you suspect a scam: don't engage, block the number/sender, report to authorities, and warn others.";
                if (lowerInput.Contains("phish") || lowerInput.Contains("phishing"))
                    return "🎣 If you suspect phishing: don't click anything, report the email to your IT department or the company being impersonated, and delete it.";
            }

            if (lowerInput.Contains("why") && (lowerInput.Contains("important") || lowerInput.Contains("need")))
            {
                if (lowerInput.Contains("2fa") || lowerInput.Contains("two factor"))
                    return "🔐 2FA is crucial because passwords alone can be stolen - 2FA ensures even if someone has your password, they can't access your account without your phone!";
                if (lowerInput.Contains("update") || lowerInput.Contains("updates"))
                    return "🔄 Updates fix security vulnerabilities. Hackers exploit outdated software - updating closes those gaps!";
                if (lowerInput.Contains("password") || lowerInput.Contains("passwords"))
                    return "🔑 Strong passwords matter because weak passwords can be cracked in seconds. Each character you add makes it exponentially harder to break!";
            }

            if (lowerInput.Contains("can you") || lowerInput.Contains("could you"))
            {
                if (lowerInput.Contains("help") || lowerInput.Contains("assist"))
                    return "🤖 Of course! I can help with:\n• Password safety tips\n• Recognizing phishing emails\n• Avoiding scams\n• Privacy protection\n• Task management (add/view tasks)\n• Cybersecurity quiz\n\nJust ask about any of these topics!";
                if (lowerInput.Contains("explain") || lowerInput.Contains("tell"))
                    return "📚 I'd be happy to explain! What specific cybersecurity topic are you interested in? Try asking about passwords, phishing, or scams.";
            }

            return null; // Return null if no NLP pattern matched
        }

        // NEW: Task Processing methods (Task 1)
        private string ProcessTaskCommand(string userInput)
        {
            string lowerInput = userInput.ToLower();

            // Check if we're awaiting reminder response
            if (_awaitingTaskReminder)
            {
                return ProcessReminderResponse(userInput);
            }

            // Add task command
            if (lowerInput.StartsWith("add task") || lowerInput.Contains("create task"))
            {
                return HandleAddTask(userInput);
            }

            // View tasks command
            if (lowerInput.Contains("view tasks") || lowerInput.Contains("show tasks") || lowerInput.Contains("list tasks"))
            {
                return HandleViewTasks();
            }

            // View pending tasks
            if (lowerInput.Contains("pending tasks") || lowerInput.Contains("incomplete tasks") || lowerInput.Contains("not done"))
            {
                return HandleViewPendingTasks();
            }

            // View completed tasks
            if (lowerInput.Contains("completed tasks") || lowerInput.Contains("done tasks") || lowerInput.Contains("finished tasks"))
            {
                return HandleViewCompletedTasks();
            }

            // Complete task command
            if (lowerInput.Contains("complete task") || lowerInput.Contains("mark as done") ||
                lowerInput.Contains("finish task") || lowerInput.Contains("done task"))
            {
                return HandleCompleteTask(userInput);
            }

            // Delete task command
            if (lowerInput.Contains("delete task") || lowerInput.Contains("remove task") || lowerInput.Contains("clear task"))
            {
                return HandleDeleteTask(userInput);
            }

            return null;
        }

        private string HandleAddTask(string userInput)
        {
            // Extract task description (remove "add task" prefix)
            string taskText = userInput;
            if (taskText.ToLower().StartsWith("add task"))
                taskText = taskText.Substring(8).Trim();
            else if (taskText.ToLower().StartsWith("create task"))
                taskText = taskText.Substring(11).Trim();

            if (string.IsNullOrWhiteSpace(taskText))
                return "What task would you like to add? Please describe the cybersecurity task.\n\nExample: 'Add task - Review privacy settings on social media'";

            _pendingTaskTitle = taskText.Length > 50 ? taskText.Substring(0, 47) + "..." : taskText;
            _pendingTaskDescription = taskText;

            _awaitingTaskReminder = true;
            _dbHelper.LogActivity("Task Creation", $"User started adding task: {_pendingTaskTitle}");

            return $"📝 Task added: \"{_pendingTaskTitle}\"\n\nWould you like to set a reminder? (reply with 'yes' to set date, 'no' for no reminder)";
        }

        private string ProcessReminderResponse(string userInput)
        {
            string lowerInput = userInput.ToLower();

            if (lowerInput.Contains("yes") || lowerInput.Contains("set reminder") || lowerInput.Contains("remind"))
            {
                return "📅 When would you like to be reminded?\n\nExamples:\n- 'tomorrow'\n- 'in 3 days'\n- 'next week'\n- 'on 2026-07-01'\n\nPlease specify a date or timeframe.";
            }
            else if (lowerInput.Contains("no") || lowerInput.Contains("skip") || lowerInput.Contains("none"))
            {
                return SaveTaskToDatabase(null);
            }
            else
            {
                // Try to parse reminder date
                DateTime? reminderDate = ParseReminderDate(lowerInput);
                if (reminderDate.HasValue)
                {
                    return SaveTaskToDatabase(reminderDate);
                }
                else
                {
                    return "I didn't understand that date format. Please try again with format like 'tomorrow', 'in 5 days', or 'no' for no reminder.";
                }
            }
        }

        private DateTime? ParseReminderDate(string input)
        {
            if (input.Contains("tomorrow") || input.Contains("tmr"))
                return DateTime.Today.AddDays(1);

            if (input.Contains("next week"))
                return DateTime.Today.AddDays(7);

            if (input.Contains("next month"))
                return DateTime.Today.AddMonths(1);

            // Parse "in X days/weeks"
            if (input.Contains("in "))
            {
                var words = input.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    if (int.TryParse(words[i], out int number))
                    {
                        // Check if next word is days, day, weeks, week, months, month
                        if (i + 1 < words.Length)
                        {
                            string nextWord = words[i + 1].ToLower();
                            if (nextWord.StartsWith("day"))
                                return DateTime.Today.AddDays(number);
                            if (nextWord.StartsWith("week"))
                                return DateTime.Today.AddDays(number * 7);
                            if (nextWord.StartsWith("month"))
                                return DateTime.Today.AddMonths(number);
                        }
                    }
                }
            }

            // Try parsing specific date
            try
            {
                if (DateTime.TryParse(input, out DateTime specificDate))
                    return specificDate;
            }
            catch
            {
                // Ignore parsing errors
            }

            return null;
        }

        private string SaveTaskToDatabase(DateTime? reminderDate)
        {
            bool success = _dbHelper.AddTask(_pendingTaskTitle, _pendingTaskDescription, reminderDate);

            _awaitingTaskReminder = false;

            if (success)
            {
                _dbHelper.LogActivity("Task Added", $"Task: {_pendingTaskTitle}");
                string reminderText = reminderDate.HasValue
                    ? $" with reminder on {reminderDate.Value:MMM dd, yyyy}"
                    : "";

                return $"✅ Task saved successfully{reminderText}!\n\nYou can view all tasks by typing 'view tasks'.";
            }
            else
            {
                return "❌ There was an error saving your task to the database. Please try again.";
            }
        }

        private string HandleViewTasks()
        {
            var tasks = _dbHelper.GetAllTasks();

            if (tasks.Count == 0)
                return "📋 You have no tasks yet. Type 'add task' to create your first cybersecurity task!\n\nExample: 'add task - Review my privacy settings'";

            string response = "📋 **YOUR CYBERSECURITY TASKS**\n\n";
            response += "═══════════════════════════════════\n\n";

            var pendingTasks = tasks.FindAll(t => t.Status == "pending");
            var completedTasks = tasks.FindAll(t => t.Status == "completed");

            if (pendingTasks.Count > 0)
            {
                response += "⏳ **Pending Tasks:**\n";
                int counter = 1;
                foreach (var task in pendingTasks)
                {
                    response += $"  {counter}. {task.StatusIcon} **{task.Title}**\n";
                    response += $"     📝 {task.Description}\n";
                    response += $"     {task.DisplayReminder}\n\n";
                    counter++;
                }
            }
            else
            {
                response += "⏳ **Pending Tasks:**\n  No pending tasks. Great job!\n\n";
            }

            if (completedTasks.Count > 0)
            {
                response += "✅ **Completed Tasks:**\n";
                foreach (var task in completedTasks.Take(5))
                {
                    response += $"  {task.StatusIcon} ~~{task.Title}~~ (completed on {task.CompletedAt:MMM dd})\n";
                }
                if (completedTasks.Count > 5)
                    response += $"     ... and {completedTasks.Count - 5} more completed tasks\n";
            }

            response += "\n═══════════════════════════════════\n";
            response += "💡 **Commands:**\n";
            response += "  • 'complete task [number]' - Mark as done\n";
            response += "  • 'delete task [number]' - Remove task\n";
            response += "  • 'add task [description]' - Create new task";

            return response;
        }

        private string HandleViewPendingTasks()
        {
            var tasks = _dbHelper.GetPendingTasks();

            if (tasks.Count == 0)
                return "🎉 Great job! You have no pending tasks. All caught up on cybersecurity!\n\nWhy not help others stay safe by sharing what you've learned?";

            string response = "⏳ **PENDING TASKS**\n\n";
            response += "═══════════════════════════════════\n\n";

            int counter = 1;
            foreach (var task in tasks)
            {
                response += $"{counter}. {task.StatusIcon} **{task.Title}**\n";
                response += $"   📝 {task.Description}\n";
                response += $"   {task.DisplayReminder}\n";
                response += $"   Created: {task.CreatedAt:MMM dd, yyyy}\n\n";
                counter++;
            }

            response += "═══════════════════════════════════\n";
            response += "💡 Type 'complete task [number]' to mark a task as done!";

            return response;
        }

        private string HandleViewCompletedTasks()
        {
            var tasks = _dbHelper.GetCompletedTasks();

            if (tasks.Count == 0)
                return "📋 No completed tasks yet. Start completing tasks to see them here!\n\nType 'view tasks' to see all your tasks.";

            string response = "✅ **COMPLETED TASKS**\n\n";
            response += "═══════════════════════════════════\n\n";

            foreach (var task in tasks)
            {
                response += $"✓ **{task.Title}**\n";
                response += $"  Completed on: {task.CompletedAt:MMM dd, yyyy}\n";
                if (!string.IsNullOrEmpty(task.Description))
                    response += $"  📝 {task.Description}\n";
                response += "\n";
            }

            response += "═══════════════════════════════════\n";
            response += "💡 Keep up the great work protecting yourself online!";

            return response;
        }

        private string HandleCompleteTask(string userInput)
        {
            var pendingTasks = _dbHelper.GetPendingTasks();

            if (pendingTasks.Count == 0)
                return "🌟 No pending tasks to complete! You're all caught up. Add a new task with 'add task' to stay organized!";

            // Try to extract task number
            int taskNumber = -1;
            var words = userInput.Split(' ');
            foreach (var word in words)
            {
                if (int.TryParse(word, out int num))
                {
                    taskNumber = num;
                    break;
                }
            }

            if (taskNumber < 1 || taskNumber > pendingTasks.Count)
            {
                string taskList = "Please specify which task to complete (by number):\n\n";
                for (int i = 0; i < Math.Min(pendingTasks.Count, 10); i++)
                {
                    taskList += $"  {i + 1}. {pendingTasks[i].Title}\n";
                }
                if (pendingTasks.Count > 10)
                    taskList += $"  ... and {pendingTasks.Count - 10} more tasks\n";
                taskList += "\nExample: 'complete task 1'";
                return taskList;
            }

            var taskToComplete = pendingTasks[taskNumber - 1];
            bool success = _dbHelper.MarkTaskAsCompleted(taskToComplete.Id);

            if (success)
            {
                _dbHelper.LogActivity("Task Completed", $"Task: {taskToComplete.Title}");
                return $"✅ Great job completing \"{taskToComplete.Title}\"! 🎉\n\nKeep up the good work protecting yourself online!\n\n💡 You have {pendingTasks.Count - 1} pending tasks remaining.";
            }
            else
            {
                return "❌ Error marking task as complete. Please try again.";
            }
        }

        private string HandleDeleteTask(string userInput)
        {
            var tasks = _dbHelper.GetAllTasks();

            if (tasks.Count == 0)
                return "📋 No tasks to delete. You're all clear!";

            int taskNumber = -1;
            var words = userInput.Split(' ');
            foreach (var word in words)
            {
                if (int.TryParse(word, out int num))
                {
                    taskNumber = num;
                    break;
                }
            }

            if (taskNumber < 1 || taskNumber > tasks.Count)
            {
                string taskList = "Please specify which task to delete (by number):\n\n";
                for (int i = 0; i < Math.Min(tasks.Count, 10); i++)
                {
                    taskList += $"  {i + 1}. {tasks[i].Title} [{tasks[i].Status}]\n";
                }
                if (tasks.Count > 10)
                    taskList += $"  ... and {tasks.Count - 10} more tasks\n";
                taskList += "\nExample: 'delete task 1'";
                return taskList;
            }

            var taskToDelete = tasks[taskNumber - 1];
            bool success = _dbHelper.DeleteTask(taskToDelete.Id);

            if (success)
            {
                _dbHelper.LogActivity("Task Deleted", $"Task: {taskToDelete.Title}");
                return $"🗑️ Task \"{taskToDelete.Title}\" has been deleted.\n\nYou have {tasks.Count - 1} tasks remaining.";
            }
            else
            {
                return "❌ Error deleting task. Please try again.";
            }
        }

        // NEW: Activity Log View (Task 4 - Preview)
        private string HandleViewActivityLog()
        {
            var activities = _dbHelper.GetRecentActivities(10);

            if (activities.Count == 0)
                return "📋 No activity log entries yet. Start interacting with me to see your activity history!\n\nTry asking about passwords, adding a task, or playing the quiz!";

            string response = "📜 **ACTIVITY LOG** (Last 10 actions)\n\n";
            response += "═══════════════════════════════════\n\n";

            foreach (var activity in activities)
            {
                string icon = activity.ActionType switch
                {
                    "Task Added" => "📝",
                    "Task Completed" => "✅",
                    "Task Deleted" => "🗑️",
                    "Task Creation" => "✏️",
                    "Keyword Query" => "🔍",
                    "Quiz Started" => "🎮",
                    "Quiz Completed" => "🏆",
                    "User Registration" => "👤",
                    "NLP Query" => "🤖",
                    _ => "•"
                };

                response += $"{icon} **{activity.ActionType}**\n";
                response += $"   {activity.Description}\n";
                response += $"   📅 {activity.Timestamp:MMM dd, yyyy HH:mm}\n\n";
            }

            response += "═══════════════════════════════════\n";
            response += "💡 Type 'activity log' again to refresh";

            return response;
        }

        // Keep all existing methods unchanged below this point
        private bool IsFollowUpRequest(string input)
        {
            string lower = input.ToLower();
            return lower.Contains("another") ||
                   lower.Contains("more") ||
                   lower.Contains("tell me more") ||
                   lower.Contains("explain") ||
                   lower.Contains("continue");
        }

        private string HandleFollowUp()
        {
            if (!string.IsNullOrEmpty(_currentTopic))
            {
                return $"Here's another tip about {_currentTopic}:\n" + GetTopicTip(_currentTopic);
            }
            return GetRandomTip();
        }

        private string HandleSentimentResponse(Sentiment sentiment, string userInput)
        {
            switch (sentiment)
            {
                case Sentiment.Worried:
                    return "I understand your concern. Cybersecurity can feel overwhelming, but taking small steps helps a lot. " +
                           "Let me share a simple tip to help you feel more secure:\n" + GetRandomTip();
                case Sentiment.Frustrated:
                    return "I'm sorry you're feeling frustrated. Let me simplify things for you.\n" + GetSimpleTip();
                case Sentiment.Curious:
                    return "That's great that you're curious! Let me share something interesting:\n" + GetRandomTip();
                default:
                    return null;
            }
        }

        private string GetTopicTip(string topic)
        {
            if (_keywordResponses.ContainsKey(topic))
            {
                var tips = _keywordResponses[topic];
                return tips[_random.Next(tips.Count)];
            }
            return GetRandomTip();
        }

        public string GetRandomTip()
        {
            var allTips = _phishingTips.Concat(_passwordTips).Concat(_generalTips).ToList();
            return allTips[_random.Next(allTips.Count)];
        }

        public string GetRandomPhishingTip()
        {
            _currentTopic = "phish";
            _dbHelper.LogActivity("Keyword Query", "User requested phishing tip");
            return _phishingTips[_random.Next(_phishingTips.Count)];
        }

        public string GetPasswordTip()
        {
            _currentTopic = "password";
            _dbHelper.LogActivity("Keyword Query", "User requested password tip");
            return _passwordTips[_random.Next(_passwordTips.Count)];
        }

        public string GetScamTip()
        {
            _currentTopic = "scam";
            var scamTips = _keywordResponses["scam"];
            _dbHelper.LogActivity("Keyword Query", "User requested scam tip");
            return scamTips[_random.Next(scamTips.Count)];
        }

        public string GetPrivacyTip()
        {
            _currentTopic = "privacy";
            var privacyTips = _keywordResponses["privacy"];
            _dbHelper.LogActivity("Keyword Query", "User requested privacy tip");
            return privacyTips[_random.Next(privacyTips.Count)];
        }

        private string GetSimpleTip()
        {
            return "🔒 Simple rule: If you didn't expect it, don't click it. If you didn't request it, don't trust it.";
        }
    }
}