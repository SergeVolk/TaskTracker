using System;
using System.Windows;

namespace TaskTracker.Client.WPF.Views
{
    public partial class InputBox : Window
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
