namespace Common_glTF_Exporter.Windows.MainWindow
{
    using System;
    using System.Reflection;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Utils;

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
                    var tempvalue = Convert.ToBoolean(DatabaseKeyValueAccesor.GetValue(propertyName));
                    preferenceType.GetProperty(propertyName).SetValue(preferences, tempvalue);
                }

                if (property.PropertyType == typeof(CompressionEnum))
                {
                    string result = DatabaseKeyValueAccesor.GetValue(propertyName).ToString();
                    Enum.TryParse(result, out CompressionEnum unitStatus);
                    preferenceType.GetProperty(propertyName).SetValue(preferences, unitStatus);
                }

                if (property.PropertyType == typeof(FormatEnum))
                {
                    string result = SettingsConfig.GetValue(propertyName).ToString();
                    Enum.TryParse(result, out FormatEnum unitStatus);
                    preferenceType.GetProperty(propertyName).SetValue(preferences, unitStatus);
                }

                if (property.PropertyType == typeof(string))
                {
                    var tempvalue = DatabaseKeyValueAccesor.GetValue(propertyName).ToString();
                    preferenceType.GetProperty(propertyName).SetValue(preferences, tempvalue);
                }

                if (
                #if REVIT2019 || REVIT2020
                property.PropertyType == typeof(DisplayUnitType))
                #else
                property.PropertyType == typeof(ForgeTypeId))
                #endif
                {
                    string result = DatabaseKeyValueAccesor.GetValue(propertyName).ToString();

                    #if REVIT2019 || REVIT2020
                    Enum.TryParse(result, out DisplayUnitType unitStatus);
                    #else
                    ForgeTypeId unitStatus = new ForgeTypeId(result);
                    #endif
                    preferenceType.GetProperty(propertyName).SetValue(preferences, unitStatus);
                }

                if (property.PropertyType == typeof(int))
                {
                    var tempvalue = Convert.ToInt32(DatabaseKeyValueAccesor.GetValue(propertyName).ToString());
                    preferenceType.GetProperty(propertyName).SetValue(preferences, tempvalue);
                }
            }

            return preferences;
        }
    }
}
