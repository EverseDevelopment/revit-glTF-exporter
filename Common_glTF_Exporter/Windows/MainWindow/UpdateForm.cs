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

            List<System.Windows.Controls.Control> children = AllChildren(border);

            foreach (PropertyInfo property in properties)
            {
                try
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

                    if (property.PropertyType == typeof(CompressionEnum))
                    {
                        List<Control> controls = children.Where(t => t is System.Windows.Controls.RadioButton).ToList();
                        List<System.Windows.Controls.RadioButton> listOfCheckboxes = controls.Cast<System.Windows.Controls.RadioButton>().ToList();
                        string value = preferenceType.GetProperty(property.Name).GetValue(preferences).ToString();
                        System.Windows.Controls.RadioButton currentCheckbox = listOfCheckboxes.FirstOrDefault(t => t.Name.Equals(value));
                        currentCheckbox.IsChecked = true;
                    }

                    if (property.PropertyType == typeof(int))
                    {
                        Slider slider = children.FirstOrDefault(t => t.Name.Equals(property.Name)) as Slider;

                        if (slider == null)
                        {
                            continue;
                        }

                        string value = preferenceType.GetProperty(property.Name).GetValue(preferences).ToString();
                        slider.Value = Convert.ToDouble(preferenceType.GetProperty(property.Name).GetValue(preferences));
                    }

                    #if REVIT2019 || REVIT2020

                    if (property.PropertyType == typeof(DisplayUnitType))
                    {
                        List<Control> controls = children.Where(t => t.Name.Equals(property.Name)).ToList();
                        System.Windows.Controls.ComboBox comboBox = controls.Cast<System.Windows.Controls.ComboBox>().First();

                        string value = preferenceType.GetProperty(property.Name).GetValue(preferences).ToString();
                        Enum.TryParse(value, out DisplayUnitType unitType);

                        var itemSource = comboBox.ItemsSource as ObservableCollection<UnitObject>;
                        UnitObject elementSel = itemSource.First(x => x.DisplayUnitType == unitType);
                        comboBox.SelectedIndex = comboBox.Items.IndexOf(elementSel);
                    }

                    #else

                    if (property.PropertyType == typeof(ForgeTypeId))
                    {
                        List<Control> controls = children.Where(t => t.Name.Equals(property.Name)).ToList();
                        System.Windows.Controls.ComboBox comboBox = controls.Cast<System.Windows.Controls.ComboBox>().First();
                        ForgeTypeId forgeTypeId = preferenceType.GetProperty(property.Name).GetValue(preferences) as ForgeTypeId;
                        UnitObject newObjt = new UnitObject(forgeTypeId);
                        var itemSource = comboBox.ItemsSource as ObservableCollection<UnitObject>;
                        UnitObject elementSel = itemSource.First(x => x.ForgeTypeId == forgeTypeId);
                        comboBox.SelectedIndex = comboBox.Items.IndexOf(elementSel);
                    }

                    #endif
                }
                catch (Exception ex)
                {
                    throw ex;
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
