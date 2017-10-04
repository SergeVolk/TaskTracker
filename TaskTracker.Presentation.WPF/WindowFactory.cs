using System;
using System.Windows;

using TaskTracker.Presentation.WPF.ViewModels;
using TaskTracker.Presentation.WPF.Views;
using TaskTracker.Repository;

namespace TaskTracker.Presentation.WPF
{
    public class WindowFactory
    {
        public Window CreateMainWindow(IRepositoryQueries repositoryQueries, ITransactionalRepositoryCommands repositoryCommands)
        {
            var mainWindowVM = new MainWindowViewModel(new UIService(), repositoryQueries, repositoryCommands);
            return new MainWindow { DataContext = mainWindowVM };
        }
    }
}
