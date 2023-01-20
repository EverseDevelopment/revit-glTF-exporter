namespace Common_glTF_Exporter.Windows.MainWindow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Autodesk.Revit.UI;
    using Common_glTF_Exporter.Utils;
    using Common_glTF_Exporter.ViewModel;
    using Revit_glTF_Exporter;

    public class LabelVersion
    {
        public static void Update(UnitsViewModel unitsViewModel)
        {
            string version = DatabaseKeyValueAccesor.GetValue("version");

            unitsViewModel.Version = version;
        }
    }
}
