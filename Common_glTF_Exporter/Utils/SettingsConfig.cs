namespace Common_glTF_Exporter.Utils
{
    using Autodesk.Internal.InfoCenter;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using Configuration = System.Configuration.Configuration;
    using Autodesk.Revit.DB;
    using System;
    using System.Xml;

    public static class SettingsConfig
    {
#if REVIT2025
        
        private static string programDataLocation = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        private static string appSettingsFile = string.Concat(programDataLocation, "\\Autodesk\\ApplicationPlugins\\leia.bundle\\Contents\\2025\\Leia_glTF_Exporter.dll.config");

        public static string GetValue(string key)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(appSettingsFile);

                XmlNode node = doc.SelectSingleNode($"//appSettings/add[@key='{key}']");
                if (node != null)
                {
                    return node.Attributes["value"].Value;
                }
                else
                {
                    throw new KeyNotFoundException($"Key '{key}' not found in configuration file.");
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new InvalidOperationException($"Error retrieving value for key '{key}'", ex);
            }
        }

        public static void SetValue(string key, string value)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(appSettingsFile);

                XmlNode node = doc.SelectSingleNode($"//appSettings/add[@key='{key}']");
                if (node != null)
                {
                    node.Attributes["value"].Value = value;
                }
                else
                {
                    XmlNode appSettingsNode = doc.SelectSingleNode("//appSettings");
                    if (appSettingsNode == null)
                    {
                        appSettingsNode = doc.CreateElement("appSettings");
                        doc.DocumentElement.AppendChild(appSettingsNode);
                    }

                    XmlElement addElement = doc.CreateElement("add");
                    addElement.SetAttribute("key", key);
                    addElement.SetAttribute("value", value);
                    appSettingsNode.AppendChild(addElement);
                }

                doc.Save(appSettingsFile);
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new InvalidOperationException($"Error setting value for key '{key}'", ex);
            }
        }

#else

        private static readonly string BinaryLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string appSettingsName = string.Concat(Assembly.GetExecutingAssembly().GetName().Name, ".dll.config");
        private static string appSettingsFile = System.IO.Path.Combine(BinaryLocation, appSettingsName);

        public static string GetValue(string key)
        {
            try
            {
                Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = appSettingsFile }, ConfigurationUserLevel.None);
                return configuration.AppSettings.Settings[key].Value;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new InvalidOperationException($"Error retrieving value for key '{key}'", ex);
            }
        }

        public static void SetValue(string key, string value)
        {
            try
            {
                Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = appSettingsFile }, ConfigurationUserLevel.None);
                configuration.AppSettings.Settings[key].Value = value;
                configuration.Save(ConfigurationSaveMode.Modified, true);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new InvalidOperationException($"Error setting value for key '{key}'", ex);
            }
        }
#endif

        private static string GetBinaryLocation()
        {
            string location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (string.IsNullOrEmpty(location))
            {
                UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
                location = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            }

            return location;
        }
    }
}
