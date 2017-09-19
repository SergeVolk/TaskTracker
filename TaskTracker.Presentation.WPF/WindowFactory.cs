using System;
using System.Windows;

using TaskTracker.Presentation.WPF.ViewModels;
using TaskTracker.Presentation.WPF.Views;
using TaskTracker.Repository;

namespace TaskTracker.Presentation.WPF
{
    public class WindowFactory
    {
        public Window CreateMainWindow(IRepository repository)
        {
            var mainWindowVM = new MainWindowViewModel(new UIService(), repository);
            return new MainWindow { DataContext = mainWindowVM };
        }
    }
}
