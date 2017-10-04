using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace TaskTracker.DBManager
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string dbLocation = "";
        private string initialDirectory;
        private string dbConnectionStringTemplate;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            initialDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDi‌​rectory, @"..\..\.."));
            dbConnectionStringTemplate = ConfigurationManager.ConnectionStrings["ConnectionStringTemplate"].ConnectionString;
        }

        public string DBLocation
        {
            get { return dbLocation; }
            private set
            {
                if (!dbLocation.Equals(value))
                {
                    dbLocation = value;
                    NotifyPropertyChanged(nameof(DBLocation));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void tbnSearchDB_Click(object sender, RoutedEventArgs e)
        {                        
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = initialDirectory;
            dialog.AddExtension = true;
            dialog.Multiselect = false;
            dialog.DefaultExt = "mdf";
         
            DBLocation = dialog.ShowDialog().GetValueOrDefault() ? dialog.FileNames.Single() : "";            
        }

        private void btnInitPreset_Click(object sender, RoutedEventArgs e)
        {
            var connectionString = BuildConnectionString(DBLocation);
            DBInitializer.InitPreset(connectionString);
        }

        protected void NotifyPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void SetProperty<TProp>(ref TProp orgValue, TProp newValue, params string[] changedProperties)
        {
            if (!EqualityComparer<TProp>.Default.Equals(orgValue, newValue))
            {
                orgValue = newValue;
                foreach (var item in changedProperties)
                {
                    NotifyPropertyChanged(item);
                }
            }
        }

        private void btnCreateDB_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.InitialDirectory = initialDirectory;
            saveDialog.AddExtension = true;
            saveDialog.DefaultExt = "mdf";

            var newDBLocation = saveDialog.ShowDialog().GetValueOrDefault() ?
                saveDialog.FileNames.Single() :
                "";

            DBLocation = newDBLocation;
            var connectionString = BuildConnectionString(DBLocation);
            DBInitializer.CreateDB(connectionString);            
        }

        private string BuildConnectionString(string dbLocation)
        {
            var sb = new StringBuilder(dbConnectionStringTemplate);
            sb.Append($";AttachDbFilename={dbLocation}");
            return sb.ToString();
        }
    }

    public class StringPathToEnableConverter : IValueConverter
    {        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
            
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !String.IsNullOrEmpty((String)value);
        }
    }
}
