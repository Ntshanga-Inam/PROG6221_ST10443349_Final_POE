using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CybersecurityChatbot_Part2.Services;

using TaskItemModel = CybersecurityChatbot_Part2.Models.TaskItem;

namespace CybersecurityChatbot_Part2.Views
{
   
    public partial class TaskManagementWindow : Window
    {
        private DatabaseHelper _dbHelper;
        private ObservableCollection<TaskItemModel> _tasks;

        public TaskManagementWindow()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _tasks = new ObservableCollection<TaskItemModel>();
            TasksListBox.ItemsSource = _tasks;
            LoadTasks();
        }

        private void LoadTasks()
        {
            try
            {
                var tasks = _dbHelper.GetAllTasks();
                _tasks.Clear();
                foreach (var task in tasks)
                {
                    _tasks.Add(task);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks();
            MessageBox.Show("Tasks refreshed successfully!", "Refresh",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CompleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TasksListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a task to mark as complete.", "Selection Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedTask = TasksListBox.SelectedItem as TaskItemModel;
            if (selectedTask == null) return;

            if (selectedTask.Status == "completed")
            {
                MessageBox.Show("This task is already completed!", "Already Done",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Mark '{selectedTask.Title}' as complete?", "Complete Task",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = _dbHelper.MarkTaskAsCompleted(selectedTask.Id);
                if (success)
                {
                    _dbHelper.LogActivity("Task Completed", $"Task: {selectedTask.Title}");
                    LoadTasks();
                    MessageBox.Show($"✅ '{selectedTask.Title}' marked as complete!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error completing task. Please try again.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TasksListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a task to delete.", "Selection Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedTask = TasksListBox.SelectedItem as TaskItemModel;
            if (selectedTask == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete '{selectedTask.Title}'?", "Delete Task",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                bool success = _dbHelper.DeleteTask(selectedTask.Id);
                if (success)
                {
                    _dbHelper.LogActivity("Task Deleted", $"Task: {selectedTask.Title}");
                    LoadTasks();
                    MessageBox.Show($"🗑️ '{selectedTask.Title}' deleted successfully!", "Deleted",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Error deleting task. Please try again.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}