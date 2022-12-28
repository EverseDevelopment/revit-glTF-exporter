using Autodesk.Revit.DB;
using Revit_glTF_Exporter;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Common_glTF_Exporter.Export
{
    public static class RevitGrids
    {
        public static void Export(Document doc, ref IndexedDictionary<glTFNode> Nodes, ref glTFNode rootNode,
            #if REVIT2019 || REVIT2020
            DisplayUnitType displayUnitType
            #else
            ForgeTypeId forgeTypeId
            #endif
            )
            {

            FilteredElementCollector col = new FilteredElementCollector(doc)
    .OfClass(typeof(Grid));

            var grids = col.ToElements();

            foreach (Grid g in grids)
            {
                Line l = g.Curve as Line;

                var origin = l.Origin;
                var direction = l.Direction;
                var length = l.Length;

                var xtras = new glTFExtras();
                var grid = new GridParameters();

                #if REVIT2019 || REVIT2020

                grid.origin = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(origin.X, displayUnitType),
                    Util.ConvertFeetToUnitTypeId(origin.Y, displayUnitType),
                    Util.ConvertFeetToUnitTypeId(origin.Z, displayUnitType) };

                grid.direction = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(direction.X, displayUnitType),
                    Util.ConvertFeetToUnitTypeId(direction.Y, displayUnitType),
                    Util.ConvertFeetToUnitTypeId(direction.Z, displayUnitType) };

                grid.length = Util.ConvertFeetToUnitTypeId(length, displayUnitType);

                #else

                    grid.origin = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(origin.X, forgeTypeId),
                    Util.ConvertFeetToUnitTypeId(origin.Y, forgeTypeId),
                    Util.ConvertFeetToUnitTypeId(origin.Z, forgeTypeId) };

                    grid.direction = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(direction.X, forgeTypeId),
                    Util.ConvertFeetToUnitTypeId(direction.Y, forgeTypeId),
                    Util.ConvertFeetToUnitTypeId(direction.Z, forgeTypeId) };

                    grid.length = Util.ConvertFeetToUnitTypeId(length, forgeTypeId);

                #endif

                xtras.GridParameters = grid;
                xtras.UniqueId = g.UniqueId;
                xtras.parameters = Util.GetElementParameters(g, true);

                var gridNode = new glTFNode();
                gridNode.name = g.Name;
                gridNode.extras = xtras;

                Nodes.AddOrUpdateCurrent(g.UniqueId, gridNode);
                rootNode.children.Add(Nodes.CurrentIndex);
            }
        }
    }
}
