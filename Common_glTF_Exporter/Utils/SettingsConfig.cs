namespace Common_glTF_Exporter.Utils
{
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using Configuration = System.Configuration.Configuration;

    public static class SettingsConfig
    {
        private static readonly string BinaryLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string appSettingsName = string.Concat(Assembly.GetExecutingAssembly().GetName().Name, ".dll.config");
        private static string appSettingsFile = System.IO.Path.Combine(BinaryLocation, appSettingsName);

        public static string GetValue(string key)
        {
            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = appSettingsFile }, ConfigurationUserLevel.None);
            return configuration.AppSettings.Settings[key].Value;
        }

        public static void SetValue(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = appSettingsFile }, ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}