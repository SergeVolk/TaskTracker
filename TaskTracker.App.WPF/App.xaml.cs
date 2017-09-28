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
            var repoFactory = new SqlRepositoryFactory(true);
            var mainWindow = new WindowFactory().CreateMainWindow(
                repoFactory.CreateRepositoryQueries(dbConnectionString), 
                repoFactory.CreateRepositoryCommands(dbConnectionString));
            mainWindow.Show();
        }        
    }
}
