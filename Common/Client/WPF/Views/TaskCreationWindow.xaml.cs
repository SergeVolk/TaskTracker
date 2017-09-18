using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace TaskTracker.Client.WPF.Views
{
    /// <summary>
    /// Interaction logic for TaskCreationWindow.xaml
    /// </summary>
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
