using System;
using System.Windows;

namespace TaskTracker.Presentation.WPF.Views
{
    partial class TaskEditorWindow : Window
    {
        public TaskEditorWindow()
        {
            InitializeComponent();
        }                

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;            
        }
    }
}
