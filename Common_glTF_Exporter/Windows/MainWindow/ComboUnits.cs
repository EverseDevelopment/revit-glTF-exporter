using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Common_glTF_Exporter.Utils;

namespace Common_glTF_Exporter.Windows.MainWindow
{
    public static class ComboUnits
    {
        public static void Set(Document doc, System.Windows.Controls.Label unitTextBlock)
        {
            string initialUnits = SettingsConfig.GetValue("units");

            #if REVIT2019 || REVIT2020

            DisplayUnitType internalProjectDisplayUnitType = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;
            unitTextBlock.Content = LabelUtils.GetLabelFor(internalProjectDisplayUnitType);
            if (initialUnits == "null")
            {
                SettingsConfig.Set("units", internalProjectDisplayUnitType.ToString());
            }

            #else

            ForgeTypeId internalProjectUnitTypeId = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
            unitTextBlock.Content = LabelUtils.GetLabelForUnit(internalProjectUnitTypeId).ToString();
            if (initialUnits == "null")
            {
                SettingsConfig.Set("units", internalProjectUnitTypeId.TypeId.ToString());
            }

            #endif
        }
    }
}
