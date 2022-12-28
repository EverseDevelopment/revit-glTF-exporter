using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Common_glTF_Exporter.ViewModel;
using Common_glTF_Exporter.Utils;
using System.IO;
using System.Threading;
using System;
using Common_glTF_Exporter.Windows.MainWindow;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using ToggleButton = System.Windows.Controls.Primitives.ToggleButton;
using System.Windows.Media;
using System.Windows.Forms;
using Control = System.Windows.Controls.Control;

namespace Common_glTF_Exporter.Windows.MainWindow
{
    public static class UpdateForm
    {
        public static void Run(Border border)
        {
            Preferences preferences = Settings.GetInfo();
            PropertyInfo[] properties = typeof(Preferences).GetProperties();
            var preferenceType = typeof(Preferences);

            List<System.Windows.Controls.Control> children = AllChildren(border);

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(bool))
                {
                    ToggleButton button = children.FirstOrDefault(t => t.Name.Equals(property.Name)) as ToggleButton;

                    if (button != null)
                    {
                        button.IsChecked = Convert.ToBoolean(preferenceType.
                            GetProperty(property.Name).GetValue(preferences));
                    }
                }

                if (property.PropertyType == typeof(CompressionEnum) )
                {
                    List<Control> controls = children.Where(t => t.Name.Contains(property.Name)).ToList();
                    List<System.Windows.Controls.RadioButton> listOfCheckboxes = controls.Cast<System.Windows.Controls.RadioButton>().ToList();
                    string value = preferenceType.GetProperty(property.Name).GetValue(preferences).ToString();
                    System.Windows.Controls.RadioButton currentCheckbox = listOfCheckboxes.FirstOrDefault(t => t.Name.Contains(value));
                    currentCheckbox.IsChecked = true;
                }
            }
        }

        private static List<System.Windows.Controls.Control> AllChildren(DependencyObject parent)
        {
            var list = new List<System.Windows.Controls.Control> { };
            for (int count = 0; count < VisualTreeHelper.GetChildrenCount(parent); count++)
            {
                var child = VisualTreeHelper.GetChild(parent, count);
                if (child is System.Windows.Controls.Control)
                {
                    list.Add(child as System.Windows.Controls.Control);
                }
                list.AddRange(AllChildren(child));
            }
            return list;
        }
    }
}
