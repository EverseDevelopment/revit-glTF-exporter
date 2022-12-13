using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Model
{
    public class UnitObject
    {
        #if REVIT2019 || REVIT2020
        public DisplayUnitType DisplayUnitType { get; internal set; }

        #else

        public ForgeTypeId ForgeTypeId { get; internal set; }

        #endif
        public string Label { get; internal set; }

        public UnitObject(

        #if REVIT2019 || REVIT2020

        DisplayUnitType displayUnitType       

        #else

        ForgeTypeId forgeTypeId           

        #endif
        )
        {
            #if REVIT2019 || REVIT2020

            DisplayUnitType = displayUnitType;
            Label = LabelUtils.GetLabelFor(DisplayUnitType);

            #else

            ForgeTypeId = forgeTypeId;
            Label = LabelUtils.GetLabelForUnit(forgeTypeId).ToString();

            #endif
        }
    }
}
