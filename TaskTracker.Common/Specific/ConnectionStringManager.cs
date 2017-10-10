using System;
using System.Configuration;
using System.IO;

namespace TaskTracker.Common
{
    public static class ConnectionStringManager
    {
        private static string csConfigName;
        private static string defaultDataDirectoryRelativePath;
        private static string dataDirectoryConfigName;

        public static void Initialize(string csConfigName, string defaultDataDirectoryRelativePath = @"..\..\..\Data", string dataDirectoryConfigName = "DataDirectory")
        {
            ConnectionStringManager.csConfigName = csConfigName;
            ConnectionStringManager.defaultDataDirectoryRelativePath = defaultDataDirectoryRelativePath;
            ConnectionStringManager.dataDirectoryConfigName = dataDirectoryConfigName;
        }

        public static string GetConnectionString()
        {
            var absoluteDataDirectory = GetDataDirectory();
            AppDomain.CurrentDomain.SetData(dataDirectoryConfigName, absoluteDataDirectory);

            return ConfigurationManager.ConnectionStrings[csConfigName].ConnectionString;
        }

        private static string GetDataDirectory()
        {
            var dataDirectory = ConfigurationManager.AppSettings[dataDirectoryConfigName];
            if (String.IsNullOrEmpty(dataDirectory))
                dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDi‌​rectory, defaultDataDirectoryRelativePath);

            return Path.GetFullPath(dataDirectory);
        }
    }
}
