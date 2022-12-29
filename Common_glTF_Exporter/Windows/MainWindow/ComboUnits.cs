using Autodesk.Revit.DB;
using Common_glTF_Exporter.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Common_glTF_Exporter.Windows.MainWindow
{
    public static class ComboUnits
    {
        public static void Set(Document doc, System.Windows.Controls.Label unitTextBlock)
        {
            string initialUnits = SettingsConfig.GetValue("units");

            #if REVIT2019 || REVIT2020

            DisplayUnitType _internalProjectDisplayUnitType = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;
            unitTextBlock.Content = LabelUtils.GetLabelFor(_internalProjectDisplayUnitType);
            if (initialUnits == "null")
            {
                SettingsConfig.Set("units", _internalProjectDisplayUnitType.ToString());
            }

            #else

            ForgeTypeId _internalProjectUnitTypeId = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
            unitTextBlock.Content = LabelUtils.GetLabelForUnit(_internalProjectUnitTypeId).ToString();
            if (initialUnits == "null")
            {
                SettingsConfig.Set("units", _internalProjectUnitTypeId.TypeId.ToString());
            }

            #endif
        }

    }
}
