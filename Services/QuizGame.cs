using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbot_Part2.Services
{
    public class QuizGame
    {
        private List<QuizQuestion> _questions;
        private int _currentQuestionIndex;
        private int _score;
        private bool _gameActive;

        
        public QuizGame()
        {
            InitializeQuestions();
            ResetGame();
        }

        private void InitializeQuestions()
        {
            _questions = new List<QuizQuestion>
            {
                // Question 1
                new QuizQuestion
                {
                    Id = 1,
                    Text = "What does 'phishing' refer to in cybersecurity?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string>
                    {
                        "A) A sport where people fish online",
                        "B) A cyberattack that tricks users into revealing sensitive information",
                        "C) A type of computer virus",
                        "D) A software update method"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation = "Phishing is a cyberattack where scammers impersonate legitimate organizations to steal sensitive information like passwords and credit card numbers."
                },
                // Question 2
                new QuizQuestion
                {
                    Id = 2,
                    Text = "Which of the following is an example of a strong password?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string>
                    {
                        "A) password123",
                        "B) PurpleDinosaur$EatsPizza!7",
                        "C) qwerty",
                        "D) 12345678"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation = "PurpleDinosaur$EatsPizza!7 is strong because it's long, uses uppercase/lowercase letters, numbers, and special characters, and isn't a common phrase."
                },
                // Question 3
                new QuizQuestion
                {
                    Id = 3,
                    Text = "Two-Factor Authentication (2FA) adds an extra layer of security. What does it typically require?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string>
                    {
                        "A) Only a password",
                        "B) A password and a second verification method (e.g., SMS code, authenticator app)",
                        "C) Biometric verification only",
                        "D) A security question only"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation = "2FA requires something you know (password) and something you have (phone for SMS/app code) or something you are (fingerprint)."
                },
                // Question 4
                new QuizQuestion
                {
                    Id = 4,
                    Text = "What should you do if you receive an unexpected email with an attachment from an unknown sender?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string>
                    {
                        "A) Open the attachment to see what it is",
                        "B) Forward it to your friends",
                        "C) Delete it immediately and do not open the attachment",
                        "D) Reply asking who sent it"
                    },
                    CorrectAnswerIndex = 2,
                    Explanation = "Never open attachments from unknown senders - they could contain malware or ransomware. Delete suspicious emails immediately."
                },
                // Question 5
                new QuizQuestion
                {
                    Id = 5,
                    Text = "A website is secure if its URL starts with:",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string>
                    {
                        "A) http://",
                        "B) ftp://",
                        "C) https://",
                        "D) www."
                    },
                    CorrectAnswerIndex = 2,
                    Explanation = "HTTPS (Hypertext Transfer Protocol Secure) encrypts data between your browser and the website, protecting your information from interception."
                },
                // Question 6
                new QuizQuestion
                {
                    Id = 6,
                    Text = "Social engineering attacks rely on:",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string>
                    {
                        "A) Technical hacking skills",
                        "B) Manipulating people into revealing confidential information",
                        "C) Advanced malware",
                        "D) Network vulnerabilities"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation = "Social engineering exploits human psychology rather than technical vulnerabilities - scammers manipulate trust, fear, or urgency."
                },
                // Question 7
                new QuizQuestion
                {
                    Id = 7,
                    Text = "You should use the same password for multiple accounts to make it easier to remember.",
                    Type = QuestionType.TrueFalse,
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "False! Using the same password across multiple accounts means if one account is compromised, all your accounts are at risk."
                },
                // Question 8
                new QuizQuestion
                {
                    Id = 8,
                    Text = "Public Wi-Fi networks are completely safe for online banking.",
                    Type = QuestionType.TrueFalse,
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "False! Public Wi-Fi is often unencrypted, making it easy for hackers to intercept your data. Use a VPN for sensitive activities."
                },
                // Question 9
                new QuizQuestion
                {
                    Id = 9,
                    Text = "What is ransomware?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string>
                    {
                        "A) Software that demands payment to unlock your files",
                        "B) A type of antivirus program",
                        "C) A secure backup solution",
                        "D) A password manager"
                    },
                    CorrectAnswerIndex = 0,
                    Explanation = "Ransomware is malware that encrypts your files and demands payment (ransom) for decryption. Never pay - maintain regular backups instead!"
                },
                // Question 10 - Bonus question 
                new QuizQuestion
                {
                    Id = 10,
                    Text = "Which of these is a sign of a potential scam email?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string>
                    {
                        "A) Urgent language like 'Your account will be closed!'",
                        "B) Spelling and grammar mistakes",
                        "C) Sender email address with small typos (e.g., support@paypa1.com)",
                        "D) All of the above"
                    },
                    CorrectAnswerIndex = 3,
                    Explanation = "All of these are red flags! Scammers create urgency, make mistakes, and use fake addresses to trick victims."
                },
                // Question 11 - Extra question
                new QuizQuestion
                {
                    Id = 11,
                    Text = "What's the best way to protect your online accounts?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string>
                    {
                        "A) Use the same simple password everywhere",
                        "B) Use a password manager and enable 2FA",
                        "C) Write passwords on sticky notes",
                        "D) Never change your passwords"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation = "Password managers generate and store strong unique passwords. Combined with 2FA, this provides excellent protection."
                }
            };
        }

        public int GetCurrentScore() => _score;
        public int GetRemainingQuestions() => _questions.Count - _currentQuestionIndex;

        public void ResetGame()
        {
            _currentQuestionIndex = 0;
            _score = 0;
            _gameActive = true;
        }

        public QuizQuestion GetCurrentQuestion()
        {
            if (_currentQuestionIndex < _questions.Count)
                return _questions[_currentQuestionIndex];
            return null;
        }

        public bool HasMoreQuestions()
        {
            return _currentQuestionIndex < _questions.Count;
        }

        public QuizResult SubmitAnswer(int selectedIndex)
        {
            var currentQuestion = GetCurrentQuestion();
            if (currentQuestion == null)
                return null;

            bool isCorrect = (selectedIndex == currentQuestion.CorrectAnswerIndex);

            if (isCorrect)
                _score++;

            var result = new QuizResult
            {
                IsCorrect = isCorrect,
                CorrectAnswer = currentQuestion.Options[currentQuestion.CorrectAnswerIndex],
                Explanation = currentQuestion.Explanation,
                CurrentScore = _score,
                TotalQuestions = _questions.Count
            };

            _currentQuestionIndex++;

            if (!HasMoreQuestions())
                _gameActive = false;

            return result;
        }

        public QuizFinalResult GetFinalResult()
        {
            int percentage = (int)((_score / (double)_questions.Count) * 100);
            string feedback;

            if (percentage >= 90)
                feedback = "🏆 Excellent! You're a cybersecurity expert! Keep spreading awareness!";
            else if (percentage >= 70)
                feedback = "🎉 Great job! You have solid cybersecurity knowledge. Keep learning!";
            else if (percentage >= 50)
                feedback = "👍 Good effort! Review the explanations to strengthen your knowledge.";
            else
                feedback = "📚 Keep learning! Cybersecurity is important for everyone. Review the tips above and try again!";

            return new QuizFinalResult
            {
                Score = _score,
                TotalQuestions = _questions.Count,
                Percentage = percentage,
                Feedback = feedback
            };
        }

        public int GetTotalQuestions() => _questions.Count;
        public bool IsGameActive => _gameActive;
    }

    public class QuizQuestion
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }

    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse
    }

    public class QuizResult
    {
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public int CurrentScore { get; set; }
        public int TotalQuestions { get; set; }
    }

    public class QuizFinalResult
    {
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public int Percentage { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }
}