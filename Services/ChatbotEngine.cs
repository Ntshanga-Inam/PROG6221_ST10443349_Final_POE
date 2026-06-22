using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        // Database Helper for Task Assistant and Activity Log
        private DatabaseHelper _dbHelper;

        // Task Assistant state tracking
        private bool _awaitingTaskReminder;
        private string _pendingTaskTitle;
        private string _pendingTaskDescription;

        // NEW: Enhanced NLP patterns for Task 3
        private Dictionary<string, List<string>> _nlpPatterns;
        private Dictionary<string, string> _nlpIntents;

        public ChatbotEngine()
        {
            _memory = new MemoryManager();
            _sentiment = new SentimentAnalyzer();
            _random = new Random();
            _lastUserTopics = new List<string>();

            _dbHelper = new DatabaseHelper();

            _awaitingTaskReminder = false;
            _pendingTaskTitle = string.Empty;
            _pendingTaskDescription = string.Empty;

            InitializeKeywordResponses();
            InitializeRandomResponsePools();
            InitializeNLPPatterns(); // NEW
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

        // NEW: Initialize NLP Patterns for Task 3
        private void InitializeNLPPatterns()
        {
            _nlpPatterns = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                // Task-related patterns
                ["add_task"] = new List<string>
                {
                    @"add\s*(?:a\s*)?task",
                    @"create\s*(?:a\s*)?task",
                    @"new\s*task",
                    @"i want to add",
                    @"i need to add",
                    @"remind me to",
                    @"remind\s*me\s*to\s*",
                    @"set\s*(?:a\s*)?reminder",
                    @"remember to",
                    @"don't forget to"
                },
                // View tasks patterns
                ["view_tasks"] = new List<string>
                {
                    @"view\s*tasks",
                    @"show\s*tasks",
                    @"list\s*tasks",
                    @"what tasks",
                    @"my tasks",
                    @"pending tasks",
                    @"incomplete tasks",
                    @"show me my tasks"
                },
                // Complete task patterns
                ["complete_task"] = new List<string>
                {
                    @"complete\s*task",
                    @"mark\s*(?:as\s*)?done",
                    @"finish\s*task",
                    @"task\s*complete",
                    @"done\s*task"
                },
                // Delete task patterns
                ["delete_task"] = new List<string>
                {
                    @"delete\s*task",
                    @"remove\s*task",
                    @"clear\s*task",
                    @"task\s*delete"
                },
                // Quiz patterns
                ["start_quiz"] = new List<string>
                {
                    @"start\s*(?:a\s*)?quiz",
                    @"play\s*(?:a\s*)?quiz",
                    @"take\s*(?:a\s*)?quiz",
                    @"test\s*me",
                    @"quiz\s*time",
                    @"i want to play",
                    @"cybersecurity\s*quiz",
                    @"knowledge\s*test"
                },
                // Activity log patterns
                ["show_log"] = new List<string>
                {
                    @"show\s*(?:activity\s*)?log",
                    @"view\s*(?:activity\s*)?log",
                    @"activity\s*log",
                    @"what have you done",
                    @"recent\s*actions",
                    @"show me what you did",
                    @"log\s*activities"
                },
                // Help patterns
                ["help"] = new List<string>
                {
                    @"help",
                    @"what can you do",
                    @"capabilities",
                    @"assist",
                    @"how can you help"
                }
            };

            _nlpIntents = new Dictionary<string, string>
            {
                ["add_task"] = "ADD_TASK",
                ["view_tasks"] = "VIEW_TASKS",
                ["complete_task"] = "COMPLETE_TASK",
                ["delete_task"] = "DELETE_TASK",
                ["start_quiz"] = "START_QUIZ",
                ["show_log"] = "SHOW_LOG",
                ["help"] = "HELP"
            };
        }

        // NEW: Enhanced NLP detection for Task 3
        private string DetectIntent(string userInput)
        {
            string lowerInput = userInput.ToLower();

            foreach (var pattern in _nlpPatterns)
            {
                foreach (var regexPattern in pattern.Value)
                {
                    // Use Regex for more sophisticated matching
                    if (Regex.IsMatch(lowerInput, regexPattern, RegexOptions.IgnoreCase))
                    {
                        return _nlpIntents[pattern.Key];
                    }
                }
            }

            // Check for simple keyword matches as fallback
            if (lowerInput.Contains("task") || lowerInput.Contains("remind") || lowerInput.Contains("remember"))
            {
                if (lowerInput.Contains("add") || lowerInput.Contains("create") || lowerInput.Contains("new") ||
                    lowerInput.Contains("remind") || lowerInput.Contains("remember"))
                {
                    return "ADD_TASK";
                }
                if (lowerInput.Contains("view") || lowerInput.Contains("show") || lowerInput.Contains("list"))
                {
                    return "VIEW_TASKS";
                }
                if (lowerInput.Contains("complete") || lowerInput.Contains("done") || lowerInput.Contains("finish"))
                {
                    return "COMPLETE_TASK";
                }
                if (lowerInput.Contains("delete") || lowerInput.Contains("remove") || lowerInput.Contains("clear"))
                {
                    return "DELETE_TASK";
                }
            }

            if (lowerInput.Contains("quiz") || lowerInput.Contains("test") || lowerInput.Contains("play"))
            {
                return "START_QUIZ";
            }

            if (lowerInput.Contains("log") || lowerInput.Contains("done") || lowerInput.Contains("action") ||
                lowerInput.Contains("recent") || lowerInput.Contains("history"))
            {
                return "SHOW_LOG";
            }

            return "UNKNOWN";
        }

        // NEW: Extract task description from NLP input
        private string ExtractTaskDescription(string userInput)
        {
            // Remove common prefixes
            string[] prefixes = {
                "add task", "create task", "new task",
                "remind me to", "set a reminder to", "remember to",
                "i want to add", "i need to add", "don't forget to",
                "add a task", "create a task"
            };

            string lowerInput = userInput.ToLower();
            string result = userInput;

            foreach (var prefix in prefixes)
            {
                if (lowerInput.StartsWith(prefix))
                {
                    result = userInput.Substring(prefix.Length).Trim();
                    break;
                }
                else if (lowerInput.Contains(prefix))
                {
                    int index = lowerInput.IndexOf(prefix);
                    result = userInput.Substring(index + prefix.Length).Trim();
                    break;
                }
            }

            // If the result is empty or the same as input, try to extract meaningful text
            if (string.IsNullOrWhiteSpace(result) || result == userInput)
            {
                // Try to extract after common trigger words
                string[] triggers = { "to", "for", "about" };
                foreach (var trigger in triggers)
                {
                    if (lowerInput.Contains(" " + trigger + " "))
                    {
                        int index = lowerInput.IndexOf(" " + trigger + " ");
                        result = userInput.Substring(index + trigger.Length + 2).Trim();
                        break;
                    }
                }
            }

            return string.IsNullOrWhiteSpace(result) ? userInput : result;
        }

        // NEW: Extract reminder date from NLP input
        private DateTime? ExtractReminderDate(string userInput)
        {
            string lowerInput = userInput.ToLower();

            // Check for "tomorrow"
            if (lowerInput.Contains("tomorrow") || lowerInput.Contains("tmr"))
                return DateTime.Today.AddDays(1);

            // Check for "next week"
            if (lowerInput.Contains("next week"))
                return DateTime.Today.AddDays(7);

            // Check for "next month"
            if (lowerInput.Contains("next month"))
                return DateTime.Today.AddMonths(1);

            // Check for "in X days/weeks"
            var match = Regex.Match(lowerInput, @"in\s*(\d+)\s*(day|week|month)s?");
            if (match.Success)
            {
                int number = int.Parse(match.Groups[1].Value);
                string unit = match.Groups[2].Value.ToLower();
                if (unit == "day")
                    return DateTime.Today.AddDays(number);
                if (unit == "week")
                    return DateTime.Today.AddDays(number * 7);
                if (unit == "month")
                    return DateTime.Today.AddMonths(number);
            }

            // Check for "on [date]"
            match = Regex.Match(lowerInput, @"on\s*(\d{4}-\d{2}-\d{2})");
            if (match.Success)
            {
                if (DateTime.TryParse(match.Groups[1].Value, out DateTime date))
                    return date;
            }

            return null;
        }

        // NEW: Main entry point with enhanced NLP
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

            // Check if we're awaiting a reminder response
            if (_awaitingTaskReminder)
            {
                return ProcessReminderResponse(userInput);
            }

            // NEW: Enhanced NLP intent detection
            string intent = DetectIntent(userInput);

            switch (intent)
            {
                case "ADD_TASK":
                    return HandleNLPAddTask(userInput);
                case "VIEW_TASKS":
                    return HandleViewTasks();
                case "COMPLETE_TASK":
                    return HandleCompleteTask(userInput);
                case "DELETE_TASK":
                    return HandleDeleteTask(userInput);
                case "START_QUIZ":
                    _dbHelper.LogActivity("Quiz Started", "User requested quiz via NLP");
                    return "QUIZ_START";
                case "SHOW_LOG":
                    return HandleViewActivityLog();
                case "HELP":
                    return GetHelpResponse();
                default:
                    break;
            }

            // Check for Quiz commands (Task 2)
            if (userInput.ToLower().Contains("quiz") || userInput.ToLower().Contains("play game") ||
                userInput.ToLower().Contains("test me") || userInput.ToLower().Contains("cybersecurity game") ||
                userInput.ToLower().Contains("start quiz"))
            {
                _dbHelper.LogActivity("Quiz Started", "User requested quiz");
                return "QUIZ_START";
            }

            // Check for Activity Log commands (Task 4)
            if (userInput.ToLower().Contains("activity log") || userInput.ToLower().Contains("show log") ||
                userInput.ToLower().Contains("view log") || userInput.ToLower().Contains("recent activities") ||
                userInput.ToLower().Contains("what have you done"))
            {
                return HandleViewActivityLog();
            }

            // Check for Task Assistant commands (Task 1)
            var taskResponse = ProcessTaskCommand(userInput);
            if (taskResponse != null)
                return taskResponse;

            // NLP Simulation (Task 3) - Advanced Natural Language Processing
            var nlpResponse = ProcessNaturalLanguage(userInput);
            if (nlpResponse != null)
            {
                _dbHelper.LogActivity("NLP Interaction", $"Processed: {userInput}");
                return nlpResponse;
            }

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

            // Default response - limited as much as possible (Task 3 requirement)
            return "I understand you're asking about cybersecurity. Could you be more specific? Try asking about passwords, phishing, scams, privacy, or say 'help' for options. You can also add tasks or start a quiz!";
        }

        // NEW: Handle NLP-based task addition
        private string HandleNLPAddTask(string userInput)
        {
            string taskDescription = ExtractTaskDescription(userInput);
            DateTime? reminderDate = ExtractReminderDate(userInput);

            if (string.IsNullOrWhiteSpace(taskDescription))
            {
                return "What task would you like to add? Please describe the cybersecurity task.";
            }

            // Clean up the task description
            taskDescription = taskDescription.Trim();
            if (taskDescription.Length > 100)
                taskDescription = taskDescription.Substring(0, 97) + "...";

            _pendingTaskTitle = taskDescription;
            _pendingTaskDescription = taskDescription;

            _dbHelper.LogActivity("Task Creation", $"User started adding task via NLP: {_pendingTaskTitle}");

            return $"📝 Task added: \"{_pendingTaskTitle}\"\n\nWould you like to set a reminder for this task? (reply with 'yes' to set date, 'no' for no reminder)";
        }

        // NEW: Get help response
        private string GetHelpResponse()
        {
            return @"🤖 **I can help you with:**

**Cybersecurity Topics:**
• 🔐 Password safety tips
• 🎣 Recognizing phishing emails  
• ⚠️ Avoiding online scams
• 🛡️ Privacy protection

**Task Management:**
• 📝 Add a task: 'Add task to enable 2FA'
• 👀 View tasks: 'Show my tasks'
• ✅ Complete tasks: 'Complete task 1'
• 🗑️ Delete tasks: 'Delete task 1'

**Fun Features:**
• 🎮 Play cybersecurity quiz: 'Start quiz'
• 📜 View activity log: 'Show activity log'
• 💡 Get random tips: 'Another tip'

**Natural Language:**
• Try phrasing things naturally like 'Remind me to update my password tomorrow'
• 'What have you done for me?' to see activity log

What would you like to do?";
        }

        // Keep original ProcessInput for backward compatibility
        public string ProcessInput(string userInput)
        {
            return ProcessInputWithFeatures(userInput);
        }

        // NEW: Enhanced NLP Simulation method (Task 3)
        private string ProcessNaturalLanguage(string userInput)
        {
            string lowerInput = userInput.ToLower();

            // Advanced NLP pattern matching with multiple variations

            // 1. "How to..." patterns
            if (lowerInput.Contains("how to") || lowerInput.Contains("way to") || lowerInput.Contains("how do i"))
            {
                if (lowerInput.Contains("protect") || lowerInput.Contains("secure") || lowerInput.Contains("safe"))
                    return "🔒 Great question! To protect yourself online:\n• Use strong, unique passwords\n• Enable 2FA on all accounts\n• Be cautious of suspicious emails\n• Keep software updated\n\nWould you like more details on any of these?";
                if (lowerInput.Contains("spot") || lowerInput.Contains("identify") && (lowerInput.Contains("phish") || lowerInput.Contains("scam")))
                    return "🎣 To spot phishing/scams:\n• Check sender email addresses carefully\n• Hover over links before clicking\n• Look for urgency or threats\n• Never share personal info via email\n\nTrust your instincts - if it feels wrong, it probably is!";
                if (lowerInput.Contains("create") && lowerInput.Contains("password"))
                    return "🔐 To create a strong password:\n• Use at least 12 characters\n• Mix uppercase and lowercase\n• Add numbers and symbols\n• Avoid common words or personal info\n• Consider a passphrase like 'PurpleDinosaur$EatsPizza!7'";
                if (lowerInput.Contains("2fa") || lowerInput.Contains("two factor"))
                    return "🔐 Setting up 2FA:\n1. Go to your account security settings\n2. Select 'Two-Factor Authentication'\n3. Choose method (SMS, Authenticator App, or Hardware Key)\n4. Follow the setup instructions\n5. Save backup codes in a safe place!";
            }

            // 2. "What if..." patterns
            if (lowerInput.Contains("what if") || lowerInput.Contains("what happens") || lowerInput.Contains("what should i"))
            {
                if (lowerInput.Contains("click") && (lowerInput.Contains("link") || lowerInput.Contains("email")))
                    return "⚠️ If you clicked a suspicious link:\n1. Disconnect from the internet immediately\n2. Run a full antivirus scan\n3. Change passwords for important accounts\n4. Monitor for unusual activity\n5. Consider reporting to the relevant authorities\n\nStay calm - acting quickly helps!";
                if (lowerInput.Contains("scam") || lowerInput.Contains("fraud"))
                    return "📞 If you suspect a scam:\n1. Don't engage with the scammer\n2. Block the number/email address\n3. Report to the appropriate authorities\n4. Warn friends and family\n5. Monitor your accounts for unusual activity\n\nRemember - legitimate organizations won't ask for sensitive info!";
                if (lowerInput.Contains("phish") || lowerInput.Contains("phishing"))
                    return "🎣 If you suspect phishing:\n1. Don't click any links or attachments\n2. Don't reply to the email\n3. Report it to your IT department or the company being impersonated\n4. Delete the email\n5. If you entered info, change your passwords immediately!";
                if (lowerInput.Contains("password") && (lowerInput.Contains("stolen") || lowerInput.Contains("hack") || lowerInput.Contains("compromised")))
                    return "🔑 If your password is compromised:\n1. Change the password immediately on that account\n2. Change it on any other accounts using the same password\n3. Enable 2FA if available\n4. Check for unauthorized activity\n5. Consider using a password manager for better security!";
            }

            // 3. "Why..." patterns
            if (lowerInput.Contains("why") && (lowerInput.Contains("important") || lowerInput.Contains("need") || lowerInput.Contains("should")))
            {
                if (lowerInput.Contains("2fa") || lowerInput.Contains("two factor"))
                    return "🔐 2FA is crucial because passwords alone can be stolen or guessed. With 2FA, even if someone has your password, they can't access your account without your second factor (phone, authenticator, or hardware key). It adds a vital extra layer of protection!";
                if (lowerInput.Contains("update") || lowerInput.Contains("updates") || lowerInput.Contains("software"))
                    return "🔄 Updates are critical because they fix security vulnerabilities that hackers exploit. Every update patches known weaknesses, protecting you from attacks. Set automatic updates where possible and never delay important security updates!";
                if (lowerInput.Contains("password") || lowerInput.Contains("passwords"))
                    return "🔑 Strong passwords matter because weak passwords can be cracked in seconds. Each additional character makes it exponentially harder to break. Think of it like a lock - a stronger lock (complex password) is harder to pick than a simple one!";
                if (lowerInput.Contains("privacy") || lowerInput.Contains("private"))
                    return "🛡️ Privacy protection matters because your personal information is valuable. Scammers can use seemingly harmless details to impersonate you, access your accounts, or steal your identity. Always think before sharing personal information online!";
            }

            // 4. "Can you..." patterns
            if (lowerInput.Contains("can you") || lowerInput.Contains("could you") || lowerInput.Contains("will you"))
            {
                if (lowerInput.Contains("help") || lowerInput.Contains("assist") || lowerInput.Contains("support"))
                    return "🤖 Absolutely! I can help with:\n• Password safety tips\n• Recognizing phishing emails\n• Avoiding scams\n• Privacy protection\n• Task management (add/view tasks)\n• Cybersecurity quiz\n• Natural language questions\n\nJust ask about any topic or try phrasing your question naturally!";
                if (lowerInput.Contains("explain") || lowerInput.Contains("tell") || lowerInput.Contains("teach"))
                    return "📚 I'd be happy to explain! What specific cybersecurity topic are you interested in? You can ask about:\n• How to create strong passwords\n• How to spot phishing emails\n• What ransomware is\n• Why 2FA is important\n• How to protect your privacy\n\nJust ask and I'll provide a detailed explanation!";
                if (lowerInput.Contains("remind") || lowerInput.Contains("remember"))
                    return "🔔 I can set reminders for you! Just tell me what you want to remember and when. For example:\n• 'Remind me to update my password tomorrow'\n• 'Remember to enable 2FA in 3 days'\n• 'Remind me to review privacy settings next week'";
                if (lowerInput.Contains("task") || lowerInput.Contains("add"))
                    return "📝 I can help you manage cybersecurity tasks! Try:\n• 'Add task to enable 2FA'\n• 'Create task to review privacy settings'\n• 'Remind me to update passwords tomorrow'\n• 'Show my pending tasks'\n\nTasks are stored in the database and you can track your progress!";
            }

            // 5. "I want to..." or "I need to..." patterns
            if (lowerInput.Contains("i want to") || lowerInput.Contains("i need to") || lowerInput.Contains("i'd like to"))
            {
                if (lowerInput.Contains("learn") || lowerInput.Contains("know") || lowerInput.Contains("understand"))
                    return "📖 That's great! What specific topic do you want to learn about? I can teach you about:\n• Password security\n• Phishing detection\n• Scam awareness\n• Privacy protection\n• Safe browsing habits\n\nJust type your topic and I'll share useful information!";
                if (lowerInput.Contains("secure") || lowerInput.Contains("protect"))
                    return "🛡️ You're doing the right thing! To stay secure:\n1. Use strong passwords\n2. Enable 2FA\n3. Be suspicious of unexpected emails\n4. Keep software updated\n5. Use antivirus software\n6. Use a VPN on public Wi-Fi\n\nNeed more details on any of these?";
                if (lowerInput.Contains("task") || lowerInput.Contains("todo") || lowerInput.Contains("to do"))
                    return "📝 I can help you manage your cybersecurity tasks! Try saying:\n• 'Add task to enable two-factor authentication'\n• 'Create task to review privacy settings'\n• 'Show my pending tasks'\n• 'Complete task 1'\n\nLet me know what you want to do!";
            }

            return null; // Return null if no NLP pattern matched
        }

        // Task Processing methods remain the same but with enhanced NLP integration
        private string ProcessTaskCommand(string userInput)
        {
            string lowerInput = userInput.ToLower();

            // Check if we're awaiting reminder response
            if (_awaitingTaskReminder)
            {
                return ProcessReminderResponse(userInput);
            }

            // Add task command with multiple variations
            if (lowerInput.StartsWith("add task") || lowerInput.Contains("create task") ||
                lowerInput.Contains("new task") || lowerInput.Contains("add a task") ||
                lowerInput.Contains("create a task"))
            {
                return HandleAddTask(userInput);
            }

            // View tasks command
            if (lowerInput.Contains("view tasks") || lowerInput.Contains("show tasks") ||
                lowerInput.Contains("list tasks") || lowerInput.Contains("my tasks"))
            {
                return HandleViewTasks();
            }

            // View pending tasks
            if (lowerInput.Contains("pending tasks") || lowerInput.Contains("incomplete tasks") ||
                lowerInput.Contains("not done") || lowerInput.Contains("incomplete"))
            {
                return HandleViewPendingTasks();
            }

            // View completed tasks
            if (lowerInput.Contains("completed tasks") || lowerInput.Contains("done tasks") ||
                lowerInput.Contains("finished tasks"))
            {
                return HandleViewCompletedTasks();
            }

            // Complete task command
            if (lowerInput.Contains("complete task") || lowerInput.Contains("mark as done") ||
                lowerInput.Contains("finish task") || lowerInput.Contains("done task") ||
                lowerInput.Contains("mark done"))
            {
                return HandleCompleteTask(userInput);
            }

            // Delete task command
            if (lowerInput.Contains("delete task") || lowerInput.Contains("remove task") ||
                lowerInput.Contains("clear task"))
            {
                return HandleDeleteTask(userInput);
            }

            return null;
        }

        private string HandleAddTask(string userInput)
        {
            // Extract task description
            string taskText = userInput;
            string[] prefixes = { "add task", "create task", "new task", "add a task", "create a task" };

            foreach (var prefix in prefixes)
            {
                if (taskText.ToLower().StartsWith(prefix))
                {
                    taskText = taskText.Substring(prefix.Length).Trim();
                    break;
                }
            }

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

            if (lowerInput.Contains("yes") || lowerInput.Contains("set reminder") || lowerInput.Contains("remind") || lowerInput.Contains("sure"))
            {
                return "📅 When would you like to be reminded?\n\nExamples:\n- 'tomorrow'\n- 'in 3 days'\n- 'next week'\n- 'on 2026-07-01'\n\nPlease specify a date or timeframe.";
            }
            else if (lowerInput.Contains("no") || lowerInput.Contains("skip") || lowerInput.Contains("none") || lowerInput.Contains("cancel"))
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

            // Parse "in X days/weeks/months"
            var match = System.Text.RegularExpressions.Regex.Match(input, @"in\s*(\d+)\s*(day|week|month)s?");
            if (match.Success)
            {
                int number = int.Parse(match.Groups[1].Value);
                string unit = match.Groups[2].Value.ToLower();
                if (unit == "day")
                    return DateTime.Today.AddDays(number);
                if (unit == "week")
                    return DateTime.Today.AddDays(number * 7);
                if (unit == "month")
                    return DateTime.Today.AddMonths(number);
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
                _dbHelper.LogActivity("Task Added", $"Task: {_pendingTaskTitle} (Reminder: {(reminderDate.HasValue ? reminderDate.Value.ToString("MMM dd, yyyy") : "None")})");
                string reminderText = reminderDate.HasValue
                    ? $" with reminder on {reminderDate.Value:MMM dd, yyyy}"
                    : "";

                if (reminderDate.HasValue)
                {
                    _dbHelper.LogActivity("Reminder Set", $"For task: {_pendingTaskTitle} on {reminderDate.Value:MMM dd, yyyy}");
                }

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

        // NEW: Enhanced Activity Log View with pagination (Task 4)
        private string HandleViewActivityLog()
        {
            var activities = _dbHelper.GetRecentActivities(10);

            if (activities.Count == 0)
                return "📋 No activity log entries yet. Start interacting with me to see your activity history!\n\nTry asking about passwords, adding a task, or playing the quiz!";

            string response = "📜 **ACTIVITY LOG** (Last 10 actions)\n\n";
            response += "═══════════════════════════════════\n\n";

            foreach (var activity in activities)
            {
                string icon = activity.Icon;
                response += $"{icon} **{activity.ActionType}**\n";
                response += $"   {activity.Description}\n";
                response += $"   📅 {activity.Timestamp:MMM dd, yyyy HH:mm}\n\n";
            }

            response += "═══════════════════════════════════\n";

            int totalCount = _dbHelper.GetTotalActivitiesCount();
            if (totalCount > 10)
            {
                response += $"💡 Showing 10 of {totalCount} activities. Type 'show more log' to see more!\n";
            }

            response += "💡 Type 'activity log' again to refresh";

            return response;
        }

        // NEW: Show more activities (Task 4)
        public string ShowMoreActivities(int page = 2)
        {
            int pageSize = 10;
            var activities = _dbHelper.GetActivitiesPaginated(page, pageSize);

            if (activities.Count == 0)
                return "📋 No more activities to show.";

            string response = $"📜 **ACTIVITY LOG** (Page {page} - {pageSize} entries)\n\n";
            response += "═══════════════════════════════════\n\n";

            foreach (var activity in activities)
            {
                string icon = activity.Icon;
                response += $"{icon} **{activity.ActionType}**\n";
                response += $"   {activity.Description}\n";
                response += $"   📅 {activity.Timestamp:MMM dd, yyyy HH:mm}\n\n";
            }

            response += "═══════════════════════════════════\n";

            int totalCount = _dbHelper.GetTotalActivitiesCount();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (page < totalPages)
            {
                response += $"💡 Page {page} of {totalPages}. Type 'show more log' to see the next page!";
            }
            else
            {
                response += "📌 You've reached the end of the activity log!";
            }

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

        // NEW: Show more activities (Task 4) - public method for MainWindow
        public string ShowMoreActivities()
        {
            return ShowMoreActivities(2);
        }
    }
}