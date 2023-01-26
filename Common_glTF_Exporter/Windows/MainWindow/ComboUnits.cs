namespace Common_glTF_Exporter.Windows.MainWindow
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Controls;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Utils;

    public static class ComboUnits
    {
        public static void Set(Document doc)
        {
            string initialUnits = DatabaseKeyValueAccesor.GetValue("units");

            #if REVIT2019 || REVIT2020

            DisplayUnitType internalProjectDisplayUnitType = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;
            if (initialUnits == "null")
            {
                DatabaseKeyValueAccesor.SetValue("units", internalProjectDisplayUnitType.ToString());
            }

            #else

            ForgeTypeId internalProjectUnitTypeId = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
            if (initialUnits == "null")
            {
                DatabaseKeyValueAccesor.SetValue("units", internalProjectUnitTypeId.TypeId.ToString());
            }

            #endif
        }
    }
}
