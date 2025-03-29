
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using UIFramework;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

namespace Common_glTF_Exporter.Utils
{
    public static class Theme
    {
        public static void ApplyDarkLightMode(ResourceDictionary resourceDictionary)
        {
            // Get the current theme
            UITheme currentTheme = Autodesk.Revit.UI.UIThemeManager.CurrentTheme;

            // Check if the theme is Dark or Light
            if (currentTheme == UITheme.Dark)
            {
                ApplyDarkMode(resourceDictionary);
            }
        }

        private static void ApplyDarkMode(ResourceDictionary resourceDictionary)
        {
            var colorMappings = new Dictionary<string, string>
            {
                { "BackgroundColor", "#18263c"},
                { "MainGray", "#ffffff"},
                { "SecondaryGray",  "#ffffff"},
                { "HoverGray", "#465162" },
                { "WhiteColour", "#18263c"},
                { "AuxiliaryGray", "#667075"}
        };

            foreach (var entry in colorMappings)
            {
                resourceDictionary[entry.Key] = new SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(entry.Value));
            }
        }
    }
}
