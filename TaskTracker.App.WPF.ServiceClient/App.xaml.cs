using System;
using System.ServiceModel;
using System.Windows;

using TaskTracker.Presentation.WPF;
using TaskTracker.Service;

namespace TaskTracker.App.WPF.ServiceClient
{
    public partial class TaskTrackerApp : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var channelFactory = new ChannelFactory<ITaskTrackerService>("TaskTrackerServiceEndpoint");
            var proxy = channelFactory.CreateChannel();
            var repository = new ServiceRepository(proxy);

            var mainWindow = new WindowFactory().CreateMainWindow(repository);
            mainWindow.Show();
        }
    }
}
