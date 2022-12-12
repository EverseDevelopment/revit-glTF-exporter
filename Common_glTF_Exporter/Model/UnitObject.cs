using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Model
{
    public class UnitObject
    {
        #if REVIT2021 || REVIT2022 || REVIT2023

        public ForgeTypeId ForgeTypeId { get; internal set; }

        #elif REVIT2019 || REVIT2020
        public DisplayUnitType DisplayUnitType { get; internal set; }
        #endif
        public string Label { get; internal set; }

        public UnitObject(

        #if REVIT2021 || REVIT2022 || REVIT2023

        ForgeTypeId forgeTypeId

        #elif REVIT2019 || REVIT2020

            DisplayUnitType displayUnitType

        #endif
        )
        {
            #if REVIT2021 || REVIT2022 || REVIT2023

            ForgeTypeId = forgeTypeId;
            Label = LabelUtils.GetLabelForUnit(forgeTypeId).ToString();

            #elif REVIT2019 || REVIT2020

            DisplayUnitType = displayUnitType;
            Label = LabelUtils.GetLabelFor(DisplayUnitType);

            #endif
        }
    }
}
