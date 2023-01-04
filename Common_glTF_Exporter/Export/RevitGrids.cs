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
                var grid = new RevitGridParametersObject();

                grid.origin = new List<double>()
                {
                    Util.ConvertFeetToUnitTypeId(origin.X, preferences),
                    Util.ConvertFeetToUnitTypeId(origin.Y, preferences),
                    Util.ConvertFeetToUnitTypeId(origin.Z, preferences),
                };

                grid.direction = new List<double>()
                {
                    Util.ConvertFeetToUnitTypeId(direction.X, preferences),
                    Util.ConvertFeetToUnitTypeId(direction.Y, preferences),
                    Util.ConvertFeetToUnitTypeId(direction.Z, preferences),
                };

                grid.length = Util.ConvertFeetToUnitTypeId(length, preferences);

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
