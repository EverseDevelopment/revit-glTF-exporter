using Autodesk.Revit.DB;
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
                    Enum.TryParse(result, out CompressionEnum myStatus);
                    preferenceType.GetProperty(propertyName).SetValue(preferences, myStatus);
                }

                if (property.PropertyType == typeof(string))
                {
                    var tempvalue = SettingsConfig.GetValue(propertyName).ToString();
                    preferenceType.GetProperty(propertyName).SetValue(preferences, tempvalue);
                }

                if (
                #if REVIT2019 || REVIT2020
                property.PropertyType == typeof(DisplayUnitType)
                #else
                property.PropertyType == typeof(ForgeTypeId)
                #endif
                 )
                {
                    string result = SettingsConfig.GetValue(propertyName).ToString();

                    #if REVIT2019 || REVIT2020
                    Enum.TryParse(result, out DisplayUnitType myStatus);
                    #else
                    ForgeTypeId myStatus = new ForgeTypeId(result);
                    #endif
                    preferenceType.GetProperty(propertyName).SetValue(preferences, myStatus);
                }

                if (property.PropertyType == typeof(int))
                {
                    var tempvalue = Convert.ToInt32(SettingsConfig.GetValue(propertyName).ToString());
                    preferenceType.GetProperty(propertyName).SetValue(preferences, tempvalue);
                }
            }

            return preferences;
        }
    }
}
