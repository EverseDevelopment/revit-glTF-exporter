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
            DisplayUnitType displayUnitType,
            #else
            ForgeTypeId forgeTypeId,
            #endif
            int decimalGrids
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
                    Util.ConvertFeetToUnitTypeId(origin.X, displayUnitType, decimalGrids),
                    Util.ConvertFeetToUnitTypeId(origin.Y, displayUnitType, decimalGrids),
                    Util.ConvertFeetToUnitTypeId(origin.Z, displayUnitType, decimalGrids) };

                grid.direction = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(direction.X, displayUnitType, decimalGrids),
                    Util.ConvertFeetToUnitTypeId(direction.Y, displayUnitType, decimalGrids),
                    Util.ConvertFeetToUnitTypeId(direction.Z, displayUnitType, decimalGrids) };

                grid.length = Util.ConvertFeetToUnitTypeId(length, displayUnitType);

#else

                grid.origin = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(origin.X, forgeTypeId, decimalGrids),
                    Util.ConvertFeetToUnitTypeId(origin.Y, forgeTypeId, decimalGrids),
                    Util.ConvertFeetToUnitTypeId(origin.Z, forgeTypeId, decimalGrids) };

                    grid.direction = new List<double>() {
                    Util.ConvertFeetToUnitTypeId(direction.X, forgeTypeId, decimalGrids),
                    Util.ConvertFeetToUnitTypeId(direction.Y, forgeTypeId, decimalGrids),
                    Util.ConvertFeetToUnitTypeId(direction.Z, forgeTypeId, decimalGrids) };

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
