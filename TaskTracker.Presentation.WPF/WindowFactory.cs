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
            var repoInitializer = repository as IRepositoryInitializer;
            if (repoInitializer != null && !repoInitializer.HasAnyData)
                repoInitializer.InitPreset();

            var mainWindowVM = new MainWindowViewModel(new UIService(), repository);
            return new MainWindow { DataContext = mainWindowVM };
        }
    }
}
