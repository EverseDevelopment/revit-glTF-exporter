namespace Common_glTF_Exporter.Windows.MainWindow
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Model;
    using Control = System.Windows.Controls.Control;
    using ToggleButton = System.Windows.Controls.Primitives.ToggleButton;

    public static class UpdateForm
    {
        public static void Run(Border border)
        {
            Preferences preferences = Settings.GetInfo();
            PropertyInfo[] properties = typeof(Preferences).GetProperties();
            var preferenceType = typeof(Preferences);

            Dictionary<string, Control> controls = GetControls(border);

            foreach (PropertyInfo property in properties)
            {
                try
                {
                    SetControlValue(property, preferences, preferenceType, controls);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private static Dictionary<string, Control> GetControls(Border border)
        {
            var controls = new Dictionary<string, Control>();
            var children = AllChildren(border);

            foreach (var child in children)
            {
                controls[child.Name] = child;
            }

            return controls;
        }

        private static void SetControlValue(PropertyInfo property, Preferences preferences, Type preferenceType, Dictionary<string, Control> controls)
        {
            switch (property.PropertyType.Name)
            {
                case "Boolean":
                    var button = controls[property.Name] as ToggleButton;
                    if (button != null)
                    {
                        button.IsChecked = Convert.ToBoolean(preferenceType.GetProperty(property.Name).GetValue(preferences));
                    }

                    break;
                case "CompressionEnum":
                case "FormatEnum":
                    var radioButton = controls.Values.OfType<System.Windows.Controls.RadioButton>().FirstOrDefault(t => t.Name.Equals(preferenceType.GetProperty(property.Name).GetValue(preferences).ToString()));
                    if (radioButton != null)
                    {
                        radioButton.IsChecked = true;
                    }

                    break;
                case "MaterialsEnum":
                    var radioButtonMaterials = controls.Values.OfType<System.Windows.Controls.RadioButton>().FirstOrDefault(t => t.Name.Equals(preferenceType.GetProperty(property.Name).GetValue(preferences).ToString()));
                    if (radioButtonMaterials != null)
                    {
                        radioButtonMaterials.IsChecked = true;
                    }

                    break;
                case "Int32":
                    var slider = controls[property.Name] as Slider;
                    if (slider != null)
                    {
                        slider.Value = Convert.ToDouble(preferenceType.GetProperty(property.Name).GetValue(preferences));
                    }

                    break;
                case "DisplayUnitType":
                case "ForgeTypeId":
                    var comboBox = controls[property.Name] as System.Windows.Controls.ComboBox;

#if REVIT2019 || REVIT2020
                    var itemSource = comboBox.ItemsSource as ObservableCollection<UnitObject>;
                    var value = preferenceType.GetProperty(property.Name).GetValue(preferences).ToString();
                    Enum.TryParse(value, out DisplayUnitType unitType);
                    var elementSel = itemSource.First(x => x.DisplayUnitType == unitType);
                    comboBox.SelectedIndex = comboBox.Items.IndexOf(elementSel);
#else
                        ForgeTypeId forgeTypeId = preferenceType.GetProperty(property.Name).GetValue(preferences) as ForgeTypeId;
                        UnitObject newObjt = new UnitObject(forgeTypeId);
                        var itemSource = comboBox.ItemsSource as ObservableCollection<UnitObject>;
                        UnitObject elementSel = itemSource.First(x => x.ForgeTypeId == forgeTypeId);
                        comboBox.SelectedIndex = comboBox.Items.IndexOf(elementSel);
#endif
                    break;
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
