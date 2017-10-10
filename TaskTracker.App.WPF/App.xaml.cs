using System;
using System.Windows;

using TaskTracker.Common;
using TaskTracker.Presentation.WPF;
using TaskTracker.Repository.Sql;

namespace TaskTracker.App.WPF
{
    public partial class TaskTrackerApp : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConnectionStringManager.Initialize("TaskTrackerDB");
            var dbConnectionString = ConnectionStringManager.GetConnectionString();

            var repoFactory = new SqlRepositoryFactory(true);
            try
            {
                var mainWindow = new WindowFactory().CreateMainWindow(
                    repoFactory.CreateRepositoryQueries(dbConnectionString),
                    repoFactory.CreateRepositoryCommands(dbConnectionString));
                mainWindow.Show();
            }
            catch (ApplicationException)
            {
                Shutdown();
            }
        }        
    }
}
