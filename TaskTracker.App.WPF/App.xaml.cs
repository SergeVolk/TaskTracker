using System;
using System.Configuration;
using System.Windows;

using TaskTracker.Presentation.WPF;
using TaskTracker.Repository.Sql;

namespace TaskTracker.App.WPF
{
    public partial class TaskTrackerApp : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dbConnectionString = ConfigurationManager.ConnectionStrings["TaskTrackerDB"].ConnectionString;            
            var mainWindow = new WindowFactory().CreateMainWindow(new SqlRepositoryFactory(true).CreateRepository(dbConnectionString));
            mainWindow.Show();
        }        
    }
}
