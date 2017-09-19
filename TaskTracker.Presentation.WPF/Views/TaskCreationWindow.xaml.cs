using System;
using System.Windows;

namespace TaskTracker.Presentation.WPF.Views
{
    partial class TaskCreationWindow : Window
    {
        public TaskCreationWindow()
        {
            InitializeComponent();
        }        

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
