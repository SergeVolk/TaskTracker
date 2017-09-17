using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using TaskTracker.Client.WPF.ViewModels;
using TaskTracker.Client.WPF.Views;
using TaskTracker.Service;

namespace TaskTracker.ServiceClient.WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var channelFactory = new ChannelFactory<ITaskTrackerService>("TaskTrackerServiceEndpoint");
            var proxy = channelFactory.CreateChannel();
            var repository = new ServiceRepository(proxy);             

            var mainWindowVM = new MainWindowViewModel(
                new UIService(),
                repository);
            var mainWindow = new MainWindow { DataContext = mainWindowVM };
            mainWindow.Show();
        }
    }
}
