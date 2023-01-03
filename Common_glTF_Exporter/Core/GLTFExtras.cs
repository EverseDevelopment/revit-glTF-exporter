namespace Common_glTF_Exporter.Core
{
    using System.Collections.Generic;
    using Revit_glTF_Exporter;

    public class GLTFExtras
    {
        /// <summary>
        /// Gets or sets the Revit created UniqueId for this object.
        /// </summary>
        /// <value>
        /// The Revit created UniqueId for this object.
        /// </value>
        public string uniqueId { get; set; }

        public GridParameters gridParameters { get; set; }

        public Dictionary<string, string> parameters { get; set; }

        public int elementId { get; set; }

        public string elementCategory { get; set; }
    }
}
