using Autodesk.Revit.DB;
using Common_glTF_Exporter.Windows.MainWindow;
using glTF.Manipulator.Schema;
using glTF.Manipulator.Utils;
using Revit_glTF_Exporter;
using System.Collections.Generic;
using System.Globalization;

namespace Common_glTF_Exporter.Export
{
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
        public static void Export(Document doc, ref IndexedDictionary<Node> nodes, ref Node rootNode, Preferences preferences)
        {
            using (FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(Grid)))
            {
                var grids = col.ToElements();
                for (int i = 0; i < grids.Count; i++)
                {
                    Grid g = grids[i] as Grid;
                    Line l = g.Curve as Line;

                    // TODO: handle Arc, not only Line
                    if (l == null)
                    {
                        continue;
                    }

                    var origin = l.Origin;
                    var direction = l.Direction;
                    var length = l.Length;

                    Extras xtras = new Extras();

                    Dictionary<string, string> parameters = preferences.properties
                        ? Util.GetElementParameters(g, true) ?? new Dictionary<string, string>()
                        : new Dictionary<string, string>();

                    parameters["UniqueId"] = g.UniqueId;

                    parameters["GridOrigin"] = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0},{1},{2}",
                        origin.X, origin.Y, origin.Z);

                    parameters["GridDirection"] = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0},{1},{2}",
                        direction.X, direction.Y, direction.Z);

                    parameters["GridLength"] = length.ToString(CultureInfo.InvariantCulture);

                    xtras.parameters = parameters;

                    Node gridNode = new Node
                    {
                        name = g.Name,
                        extras = xtras
                    };

                    nodes.AddOrUpdateCurrent(g.UniqueId, gridNode);
                    rootNode.children.Add(nodes.CurrentIndex);
                }
            }
        }
    }
}
