namespace Common_glTF_Exporter.Export
{
    using System.Collections.Generic;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Windows.MainWindow;
    using Revit_glTF_Exporter;

    /// <summary>
    /// Revit grids.
    /// </summary>
    public static class RevitGrids
    {
        /// <summary>
        /// Export Revit grids.
        /// </summary>
        /// <param name="doc">Revit document.</param>
        /// <param name="nodes">Nodes.</param>
        /// <param name="rootNode">root node.</param>
        /// <param name="preferences">preferences. </param>
        public static void Export(Document doc, ref IndexedDictionary<GLTFNode> nodes, ref GLTFNode rootNode, Preferences preferences)
        {
            #if REVIT2019 || REVIT2020
            DisplayUnitType displayUnitType = preferences.units;
            #else
            ForgeTypeId forgeTypeId = preferences.units;
            #endif

            FilteredElementCollector col = new FilteredElementCollector(doc)
                .OfClass(typeof(Grid));

            var grids = col.ToElements();

            foreach (Grid g in grids)
            {
                Line l = g.Curve as Line;

                var origin = l.Origin;
                var direction = l.Direction;
                var length = l.Length;

                var xtras = new GLTFExtras();
                var grid = new RevitGridParameters();

                #if REVIT2019 || REVIT2020

                grid.origin = new List<double>()
                {
                    Util.ConvertFeetToUnitTypeId(origin.X, displayUnitType, preferences.digits),
                    Util.ConvertFeetToUnitTypeId(origin.Y, displayUnitType, preferences.digits),
                    Util.ConvertFeetToUnitTypeId(origin.Z, displayUnitType, preferences.digits),
                };

                grid.direction = new List<double>()
                {
                    Util.ConvertFeetToUnitTypeId(direction.X, displayUnitType, preferences.digits),
                    Util.ConvertFeetToUnitTypeId(direction.Y, displayUnitType, preferences.digits),
                    Util.ConvertFeetToUnitTypeId(direction.Z, displayUnitType, preferences.digits),
                };

                grid.length = Util.ConvertFeetToUnitTypeId(length, displayUnitType, preferences.digits);

                #else

                grid.origin = new List<double>()
                {
                Util.ConvertFeetToUnitTypeId(origin.X, forgeTypeId, preferences.digits),
                Util.ConvertFeetToUnitTypeId(origin.Y, forgeTypeId, preferences.digits),
                Util.ConvertFeetToUnitTypeId(origin.Z, forgeTypeId, preferences.digits),
                };

                grid.direction = new List<double>()
                {
                Util.ConvertFeetToUnitTypeId(direction.X, forgeTypeId, preferences.digits),
                Util.ConvertFeetToUnitTypeId(direction.Y, forgeTypeId, preferences.digits),
                Util.ConvertFeetToUnitTypeId(direction.Z, forgeTypeId, preferences.digits),
                };

                grid.length = Util.ConvertFeetToUnitTypeId(length, forgeTypeId, preferences.digits);

                #endif

                xtras.gridParameters = grid;
                xtras.uniqueId = g.UniqueId;

                if (preferences.properties)
                {
                    xtras.parameters = Util.GetElementParameters(g, true);
                }

                var gridNode = new GLTFNode();
                gridNode.name = g.Name;
                gridNode.extras = xtras;

                nodes.AddOrUpdateCurrent(g.UniqueId, gridNode);
                rootNode.children.Add(nodes.CurrentIndex);
            }
        }
    }
}
