using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.Revit.DB;

namespace Common_glTF_Exporter.Model
{
    public class UnitObject
    {
        public UnitObject(
        #if REVIT2019 || REVIT2020
        DisplayUnitType displayUnitType
        #else
        ForgeTypeId forgeTypeId
        #endif
        )
                {
        #if REVIT2019 || REVIT2020

            this.DisplayUnitType = displayUnitType;
            this.Label = LabelUtils.GetLabelFor(DisplayUnitType);

        #else

            ForgeTypeId = forgeTypeId;
            Label = LabelUtils.GetLabelForUnit(forgeTypeId).ToString();

        #endif
        }

        #if REVIT2019 || REVIT2020
        public DisplayUnitType DisplayUnitType { get; internal set; }
        #else

        public ForgeTypeId ForgeTypeId { get; internal set; }

        #endif
        public string Label { get; internal set; }
    }
}
