using CybersecurityChatbot_Part2.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CybersecurityChatbot_Part2_WPF
{
    public partial class MainWindow : Window
    {
        private ChatbotEngine? _chatbot;
        private ObservableCollection<ChatMessage> _chatMessages;
        private string _userName;
        private bool _waitingForName;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize collections and variables
            _chatMessages = new ObservableCollection<ChatMessage>();
            _userName = string.Empty;
            _waitingForName = true;

            InitializeChatbot();
            DisplayAsciiArt();
            PlayVoiceGreeting();
            ShowWelcomeMessage();
        }

        private void InitializeChatbot()
        {
            _chatbot = new ChatbotEngine();
            ChatHistoryListBox.ItemsSource = _chatMessages;
        }

        private void DisplayAsciiArt()
        {
            string asciiArt = @"
╔══════════════════════════════════════════════════════════════╗
║                     CYBERSECURITY AWARENESS                  ║
║                         PROTECT YOURSELF                     ║
║    ╔═══╗╔═══╗╔╗ ╔╗╔═══╗╔═══╗╔══╗╔═══╗╔═══╗╔╗   ╔═══╗       ║
║    ║╔═╗║║╔═╗║║║ ║║║╔══╝║╔═╗║╚╣╠╝║╔══╝║╔═╗║║║   ║╔══╝       ║
║    ║╚═╝║║║ ║║║╚═╝║║╚══╗║╚═╝║ ║║ ║╚══╗║╚═╝║║║   ║╚══╗       ║
║    ║╔╗╔╝║╚═╝║╚═╗╔╝║╔══╝║╔╗╔╝ ║║ ║╔══╝║╔╗╔╝║║ ╔╗║╔══╝       ║
║    ║║║╚╗║╔═╗║ ╔╝╚╗║╚══╗║║║╚╗╔╣╠╗║╚══╗║║║╚╗║╚═╝║║╚══╗       ║
║    ╚╝╚═╝╚╝ ╚╝ ╚══╝╚═══╝╚╝╚═╝╚══╝╚═══╝╚╝╚═╝╚═══╝╚═══╝       ║
║              Stay Safe | Stay Secure | Stay Smart           ║
╚══════════════════════════════════════════════════════════════╝";

            AsciiArtBlock.Text = asciiArt;
        }

        [SupportedOSPlatform("windows")]
        private void PlayVoiceGreeting()
        {
            try
            {
                // Only attempt to play audio on Windows operating system
                if (OperatingSystem.IsWindows())
                {
                    string[] possiblePaths = new[]
                    {
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "greeting.wav"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio", "greeting.wav")
                    };

                    string? audioPath = null;
                    foreach (var path in possiblePaths)
                    {
                        if (File.Exists(path))
                        {
                            audioPath = path;
                            break;
                        }
                    }

                    if (audioPath != null)
                    {
                        using (SoundPlayer player = new SoundPlayer(audioPath))
                        {
                            player.Play(); // Play asynchronously
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail if audio is not available - application continues normally
            }
        }

        private void ShowWelcomeMessage()
        {
            AddBotMessage("Hello! Welcome to the Cybersecurity Awareness Bot.");
            AddBotMessage("What's your name?");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                e.Handled = true;
                ProcessUserInput();
            }
        }

        // NEW: Handle chatbot response including special commands like QUIZ_START
        private void HandleChatbotResponse(string response)
        {
            if (response == "QUIZ_START")
            {
                // Launch the Quiz Window
                var quizWindow = new QuizWindow();
                quizWindow.Owner = this;
                quizWindow.ShowDialog();
                AddBotMessage("🎮 Welcome back! Ready for more cybersecurity learning? Type 'quiz' anytime to play again!");
            }
            else
            {
                AddBotMessage(response);
            }
        }

        // NEW: Handle "Show More" for activity log
        private void HandleShowMoreActivities()
        {
            if (_chatbot != null)
            {
                string response = _chatbot.ShowMoreActivities();
                AddBotMessage(response);
            }
            else
            {
                AddBotMessage("I'm having trouble connecting. Please try again later.");
            }
        }

        private void ProcessUserInput()
        {
            string userInput = InputTextBox.Text.Trim();

            // Validate input
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return;
            }

            // Add user message to chat
            AddUserMessage(userInput);
            InputTextBox.Clear();

            // Handle name collection (first interaction)
            if (_waitingForName)
            {
                _userName = userInput;
                _waitingForName = false;
                string response = $"Nice to meet you, {_userName}! I'm your Cybersecurity Awareness Bot. What would you like to learn about today? You can ask about passwords, phishing, scams, privacy, tasks, or type 'quiz' to play a game!";
                AddBotMessage(response);

                // Auto-scroll to bottom
                ScrollToBottom();
                return;
            }

            // Check for "show more log" command (Task 4)
            string lowerInput = userInput.ToLower();
            if (lowerInput.Contains("show more log") || lowerInput.Contains("more log") ||
                lowerInput.Contains("show more") && lowerInput.Contains("log"))
            {
                HandleShowMoreActivities();
                ScrollToBottom();
                return;
            }

            // Process via chatbot engine with all features
            if (_chatbot != null)
            {
                // Use ProcessInputWithFeatures to handle all new features (tasks, quiz, NLP)
                string botResponse = _chatbot.ProcessInputWithFeatures(userInput);
                HandleChatbotResponse(botResponse);
            }
            else
            {
                AddBotMessage("I'm having trouble connecting. Please try again later.");
            }

            // Auto-scroll to bottom
            ScrollToBottom();
        }

        private void AddUserMessage(string message)
        {
            _chatMessages.Add(new ChatMessage
            {
                Sender = "👤 You",
                Message = message,
                Color = "#3498DB",
                BackgroundColor = "#EBF5FB",
                TextColor = "#2C3E50"
            });
        }

        private void AddBotMessage(string message)
        {
            _chatMessages.Add(new ChatMessage
            {
                Sender = "🤖 Bot",
                Message = message,
                Color = "#27AE60",
                BackgroundColor = "#FFFFFF",
                TextColor = "#2C3E50"
            });
        }

        private void ScrollToBottom()
        {
            // Use dispatcher to ensure UI is updated before scrolling
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (ChatScrollViewer != null)
                {
                    ChatScrollViewer.ScrollToBottom();
                }
            }));
        }

        // Quick action button handlers
        private void PasswordTipBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUserMessage("Tell me about password safety");
            if (_chatbot != null)
            {
                AddBotMessage(_chatbot.GetPasswordTip());
            }
            ScrollToBottom();
        }

        private void PhishingTipBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUserMessage("Give me a phishing tip");
            if (_chatbot != null)
            {
                AddBotMessage(_chatbot.GetRandomPhishingTip());
            }
            ScrollToBottom();
        }

        private void ScamTipBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUserMessage("Tell me about scams");
            if (_chatbot != null)
            {
                AddBotMessage(_chatbot.GetScamTip());
            }
            ScrollToBottom();
        }

        private void PrivacyTipBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUserMessage("Tell me about privacy");
            if (_chatbot != null)
            {
                AddBotMessage(_chatbot.GetPrivacyTip());
            }
            ScrollToBottom();
        }

        private void AnotherTipBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUserMessage("Give me another tip");
            if (_chatbot != null)
            {
                AddBotMessage(_chatbot.GetRandomTip());
            }
            ScrollToBottom();
        }

        private void ClearChatBtn_Click(object sender, RoutedEventArgs e)
        {
            // Clear all messages
            _chatMessages.Clear();

            // Reset conversation state
            _waitingForName = true;
            _userName = string.Empty;

            // Show welcome message again
            ShowWelcomeMessage();
            ScrollToBottom();
        }

        private void VoiceButton_Click(object sender, RoutedEventArgs e)
        {
            PlayVoiceGreeting();
        }
    }

    public class ChatMessage : INotifyPropertyChanged
    {
        private string _sender = string.Empty;
        private string _message = string.Empty;
        private string _color = string.Empty;
        private string _backgroundColor = string.Empty;
        private string _textColor = string.Empty;

        public string Sender
        {
            get => _sender;
            set
            {
                _sender = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public string Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }

        public string BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }

        public string TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}