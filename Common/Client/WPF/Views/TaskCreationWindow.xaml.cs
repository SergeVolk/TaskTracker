using System;
using System.Windows;

namespace TaskTracker.Client.WPF.Views
{
    public partial class TaskCreationWindow : Window
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
