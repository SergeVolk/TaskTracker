using System;
using System.Windows;

namespace TaskTracker.Client.WPF.Views
{
    public partial class TaskEditorWindow : Window
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
