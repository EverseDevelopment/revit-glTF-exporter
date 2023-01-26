using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Common_glTF_Exporter.Windows.MainWindow;
using Revit_glTF_Exporter;

namespace Common_glTF_Exporter.Transform
{
    public static class ModelScale
    {
        public static List<double> Get(Preferences preferences)
        {
            double scale = Util.ConvertFeetToUnitTypeId(preferences);
            return new List<double> { scale, scale, scale };
        }
    }
}
