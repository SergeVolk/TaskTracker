using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TaskTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DbConnectionString = ConfigurationManager.ConnectionStrings["TaskTrackerModelContainer"].ConnectionString;
        }

        public static string DbConnectionString { get; private set; }
    }
}
