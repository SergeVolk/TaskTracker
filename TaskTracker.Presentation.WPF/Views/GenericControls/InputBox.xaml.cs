using System;
using System.Windows;

namespace TaskTracker.Presentation.WPF.Views
{
    partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();            
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;            
        }
    }
}
