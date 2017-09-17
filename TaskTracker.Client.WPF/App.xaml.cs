﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TaskTracker.Client.WPF.ViewModels;
using TaskTracker.Client.WPF.Views;
using TaskTracker.Repository.Sql;

namespace TaskTracker.Client.WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dbConnectionString = ConfigurationManager.ConnectionStrings["TaskTrackerModelContainer"].ConnectionString;

            var mainWindowVM = new MainWindowViewModel(
                new UIService(), 
                new SqlRepositoryFactory().CreateRepository(dbConnectionString));
            var mainWindow = new MainWindow { DataContext = mainWindowVM };
            mainWindow.Show();
        }        
    }
}