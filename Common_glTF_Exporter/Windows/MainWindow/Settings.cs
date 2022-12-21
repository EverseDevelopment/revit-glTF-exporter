using Common_glTF_Exporter.Utils;
using System;
using System.Reflection;


namespace Common_glTF_Exporter.Windows.MainWindow
{
    public static class Settings
    {
        public static Preferences GetInfo()
        {
            Preferences preferences = new Preferences();
            PropertyInfo[] properties = typeof(Preferences).GetProperties();
            var preferenceType = typeof(Preferences);

            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name;

                if (property.PropertyType == typeof(bool))
                {    
                    var tempvalue = Convert.ToBoolean(SettingsConfig.GetValue(propertyName));
                    preferenceType.GetProperty(propertyName).SetValue(preferences, tempvalue);
                }

                if (property.PropertyType == typeof(CompressionEnum))
                {
                    string result = SettingsConfig.GetValue(propertyName).ToString();
                    var tempvalue = Enum.TryParse(result, out CompressionEnum myStatus);
                    preferenceType.GetProperty(propertyName).SetValue(preferences, myStatus);
                }

                if (property.PropertyType == typeof(string))
                {
                    var tempvalue = SettingsConfig.GetValue(propertyName).ToString();
                    preferenceType.GetProperty(propertyName).SetValue(preferences, tempvalue);
                }
            }

            return preferences;
        }
    }
}
