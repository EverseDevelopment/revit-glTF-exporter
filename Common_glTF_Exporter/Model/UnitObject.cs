using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Model
{
    public class UnitObject
    {
        public ForgeTypeId ForgeTypeId { get; internal set; }
        public string Label { get; internal set; }

        public UnitObject(ForgeTypeId forgeTypeId)
        {
            ForgeTypeId = forgeTypeId;
            Label = LabelUtils.GetLabelForUnit(forgeTypeId).ToString();
        }
    }
}
