using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CybersecurityChatbot_Part2.Services;

namespace CybersecurityChatbot_Part2_WPF
{
    public partial class QuizWindow : Window
    {
        private QuizGame _quizGame;
        private DatabaseHelper _dbHelper;
        private bool _awaitingNextQuestion;
        private int _currentSelectedOption;
        private bool _isQuizInitialized;

        public QuizWindow()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _quizGame = new QuizGame();
            _awaitingNextQuestion = false;
            _currentSelectedOption = -1;
            _isQuizInitialized = true;
            ShowStartScreen();
        }

        private void ShowStartScreen()
        {
            StartScreen.Visibility = Visibility.Visible;
            QuestionScreen.Visibility = Visibility.Collapsed;
            FeedbackScreen.Visibility = Visibility.Collapsed;
            ResultsScreen.Visibility = Visibility.Collapsed;
            ScoreTextBlock.Text = "Score: 0 / 0";
            ProgressBar.Value = 0;
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _quizGame = new QuizGame(); // Reinitialize to ensure fresh state
                _awaitingNextQuestion = false;
                _currentSelectedOption = -1;
                _dbHelper.LogActivity("Quiz Started", "User started cybersecurity quiz");
                ShowNextQuestion();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting quiz: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowNextQuestion()
        {
            try
            {
                if (_quizGame == null)
                {
                    MessageBox.Show("Quiz not initialized. Please restart the quiz.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!_quizGame.HasMoreQuestions())
                {
                    ShowResults();
                    return;
                }

                var question = _quizGame.GetCurrentQuestion();
                if (question == null)
                {
                    ShowResults();
                    return;
                }

                // Update UI
                StartScreen.Visibility = Visibility.Collapsed;
                QuestionScreen.Visibility = Visibility.Visible;
                FeedbackScreen.Visibility = Visibility.Collapsed;
                ResultsScreen.Visibility = Visibility.Collapsed;

                // Update question display
                int currentNum = _quizGame.GetTotalQuestions() - _quizGame.GetRemainingQuestions() + 1;
                int totalNum = _quizGame.GetTotalQuestions();
                QuestionNumberText.Text = $"Question {currentNum} of {totalNum}";
                QuestionTextBlock.Text = question.Text;

                // Update score display
                int currentScore = _quizGame.GetCurrentScore();
                ScoreTextBlock.Text = $"Score: {currentScore} / {totalNum}";
                ProgressBar.Value = ((double)currentScore / totalNum) * 100;

                // Display options
                OptionsListControl.ItemsSource = question.Options;
                _awaitingNextQuestion = false;
                _currentSelectedOption = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading question: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OptionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_quizGame == null)
                {
                    MessageBox.Show("Quiz not initialized. Please restart the quiz.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_awaitingNextQuestion)
                {
                    MessageBox.Show("Please click 'Next Question' to continue.", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var button = sender as Button;
                if (button == null) return;

                // Get the selected index from the Tag property
                int selectedIndex = -1;
                if (button.Tag != null)
                {
                    selectedIndex = Convert.ToInt32(button.Tag);
                }
                else
                {
                    // Fallback: try to find index from ItemsControl
                    var parent = button.Parent as ItemsControl;
                    if (parent != null)
                    {
                        selectedIndex = parent.Items.IndexOf(button.Content);
                    }
                }

                if (selectedIndex < 0)
                {
                    MessageBox.Show("Error: Could not determine selected option.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _currentSelectedOption = selectedIndex;

                var result = _quizGame.SubmitAnswer(selectedIndex);

                if (result != null)
                {
                    ShowFeedback(result);
                }
                else
                {
                    MessageBox.Show("Error processing your answer. Please try again.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing answer: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowFeedback(QuizResult result)
        {
            try
            {
                QuestionScreen.Visibility = Visibility.Collapsed;
                FeedbackScreen.Visibility = Visibility.Visible;

                if (result.IsCorrect)
                {
                    FeedbackBorder.Background = new SolidColorBrush(Color.FromRgb(232, 248, 245)); // Light green
                    FeedbackIcon.Text = "✅ CORRECT!";
                    FeedbackTextBlock.Text = "Great job! That's the right answer!";
                    FeedbackTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
                }
                else
                {
                    FeedbackBorder.Background = new SolidColorBrush(Color.FromRgb(253, 236, 234)); // Light red
                    FeedbackIcon.Text = "❌ INCORRECT";
                    FeedbackTextBlock.Text = "Not quite right. Let's learn from this!";
                    FeedbackTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                }

                CorrectAnswerBlock.Text = $"Correct answer: {result.CorrectAnswer}";
                ExplanationBlock.Text = result.Explanation;

                // Update score display
                int total = _quizGame.GetTotalQuestions();
                ScoreTextBlock.Text = $"Score: {result.CurrentScore} / {total}";
                ProgressBar.Value = ((double)result.CurrentScore / total) * 100;

                _awaitingNextQuestion = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing feedback: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_awaitingNextQuestion)
                {
                    // If not awaiting next, just show next question
                    ShowNextQuestion();
                }
                else
                {
                    _awaitingNextQuestion = false;
                    ShowNextQuestion();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading next question: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowResults()
        {
            try
            {
                if (_quizGame == null)
                {
                    MessageBox.Show("Quiz not initialized. Please restart the quiz.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var finalResult = _quizGame.GetFinalResult();

                StartScreen.Visibility = Visibility.Collapsed;
                QuestionScreen.Visibility = Visibility.Collapsed;
                FeedbackScreen.Visibility = Visibility.Collapsed;
                ResultsScreen.Visibility = Visibility.Visible;

                // Log completion
                _dbHelper.LogActivity("Quiz Completed",
                    $"Score: {finalResult.Score}/{finalResult.TotalQuestions} ({finalResult.Percentage}%)");

                // Set results display
                if (finalResult.Percentage >= 90)
                {
                    ResultIcon.Text = "🏆";
                    ResultTitle.Text = "EXCELLENT!";
                    ResultTitle.Foreground = new SolidColorBrush(Color.FromRgb(241, 196, 15));
                }
                else if (finalResult.Percentage >= 70)
                {
                    ResultIcon.Text = "🎉";
                    ResultTitle.Text = "GOOD JOB!";
                    ResultTitle.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
                }
                else if (finalResult.Percentage >= 50)
                {
                    ResultIcon.Text = "👍";
                    ResultTitle.Text = "GOOD EFFORT!";
                    ResultTitle.Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219));
                }
                else
                {
                    ResultIcon.Text = "📚";
                    ResultTitle.Text = "KEEP LEARNING!";
                    ResultTitle.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                }

                ScoreDisplay.Text = $"Your Score: {finalResult.Score} / {finalResult.TotalQuestions}";
                PercentageDisplay.Text = $"({finalResult.Percentage}%)";
                FinalFeedbackBlock.Text = finalResult.Feedback;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing results: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _quizGame = new QuizGame(); // Reinitialize
                _awaitingNextQuestion = false;
                _currentSelectedOption = -1;
                ShowNextQuestion();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restarting quiz: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToChat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Clean up resources when window closes
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _quizGame = null;
        }
    }
}