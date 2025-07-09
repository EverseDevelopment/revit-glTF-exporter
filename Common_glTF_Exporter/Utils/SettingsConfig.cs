using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;

namespace Common_glTF_Exporter.Utils
{
    public static class SettingsConfig
    {
        private static readonly string _configDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Leia");

        private static readonly string _configFile =
            Path.Combine(_configDir, "leia.config");

        private static readonly string _currentVersion = "0.0.0";
        private static readonly string _currentApiKey= "PlaceHolderApiKey";

        private static readonly object _locker = new object();

        static SettingsConfig()
        {
            // Ensure folder exists
            if (!Directory.Exists(_configDir))
                Directory.CreateDirectory(_configDir);

            // Ensure file exists with defaults
            if (!File.Exists(_configFile))
                CreateDefaultConfig();
        }

        public static string GetValue(string key)
        {
            lock (_locker)
            {
                try
                {
                    Configuration config = OpenConfig();
                    KeyValueConfigurationElement setting = config.AppSettings.Settings[key];
                    return setting != null ? setting.Value : null;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Error retrieving value for key '{key}'.", ex);
                }
            }
        }

        public static void SetValue(string key, string value)
        {
            lock (_locker)
            {
                try
                {
                    Configuration config = OpenConfig();
                    KeyValueConfigurationCollection settings = config.AppSettings.Settings;

                    if (settings[key] == null)
                        settings.Add(key, value);
                    else
                        settings[key].Value = value;

                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Error setting value for key '{key}'.", ex);
                }
            }
        }
        private static Configuration OpenConfig()
        {
            var map = new ExeConfigurationFileMap { ExeConfigFilename = _configFile };
            return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
        }

        /// <summary>
        /// Creates leia.config with the requested default keys/values.
        /// </summary>
        private static void CreateDefaultConfig()
        {
            // Your requested defaults (removed the duplicated "runs" key to avoid errors).
            var defaults = new Dictionary<string, string>
            {
                { "materials",   "true"  },
                { "format",      "gltf"  },
                { "normals",     "false" },
                { "levels",      "false" },
                { "lights",      "false" },
                { "grids",       "false" },
                { "batchId",     "false" },
                { "properties",  "false" },
                { "relocateTo0", "false" },
                { "flipAxis",    "true"  },
                { "units",       "null"  },
                { "compression", "none"  },
                { "path",        Environment.GetFolderPath(Environment.SpecialFolder.Desktop) },
                { "fileName",    "3dExport" },
                { "runs",        "0" },
                { "version",     _currentVersion },
                { "user",        "user01" },
                { "release",     "0" },
                { "isRFA",       "false" },
                { "apikey",      _currentApiKey }
            };

            var doc = new XmlDocument();
            var decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(decl);

            XmlElement configuration = doc.CreateElement("configuration");
            doc.AppendChild(configuration);

            XmlElement appSettings = doc.CreateElement("appSettings");
            configuration.AppendChild(appSettings);

            foreach (var kvp in defaults)
            {
                XmlElement add = doc.CreateElement("add");
                add.SetAttribute("key", kvp.Key);
                add.SetAttribute("value", kvp.Value);
                appSettings.AppendChild(add);
            }

            doc.Save(_configFile);
        }
    }
}
