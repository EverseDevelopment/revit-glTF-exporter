﻿namespace Common_glTF_Exporter.Windows.MainWindow
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Controls;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Model;
    using Common_glTF_Exporter.Utils;

    public static class ComboUnits
    {
        public static void Set(Document doc)
        {
            string initialUnits = SettingsConfig.GetValue("units");

            #if REVIT2019 || REVIT2020

            if (initialUnits == "null")
            {
                DisplayUnitType unit = DisplayUnitType.DUT_METERS;
                SettingsConfig.SetValue("units", unit.ToString());
            }

            #else

            if (initialUnits == "null")
            {
                ForgeTypeId unit = UnitTypeId.Meters;
                SettingsConfig.SetValue("units", unit.TypeId.ToString());
            }

            #endif
        }
    }
}
