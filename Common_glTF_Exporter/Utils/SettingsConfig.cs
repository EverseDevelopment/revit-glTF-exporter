using Autodesk.Internal.InfoCenter;
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Configuration = System.Configuration.Configuration;
using System.Reflection;
using System.IO;

namespace Common_glTF_Exporter.Utils
{
    /// <summary>
    /// SettingsConfig
    /// </summary>
    public static class SettingsConfig
    {
        private static readonly string binaryLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string appSettingsFile = System.IO.Path.Combine(binaryLocation, "Revit_glTF_Exporter.dll.config");

        public static string GetValue(string key)
        {
            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = appSettingsFile }, ConfigurationUserLevel.None);
            return configuration.AppSettings.Settings[key].Value;
        }

        public static void Set(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = appSettingsFile }, ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
