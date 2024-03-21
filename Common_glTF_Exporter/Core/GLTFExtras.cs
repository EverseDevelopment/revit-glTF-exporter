namespace Common_glTF_Exporter.Core
{
    using System.Collections.Generic;
    using Common_glTF_Exporter.Export;
    using Revit_glTF_Exporter;

    public class GLTFExtras
    {
        /// <summary>
        /// Gets or sets the Revit created UniqueId for this object.
        /// </summary>
        public string uniqueId { get; set; }

        public RevitGridParametersObject gridParameters { get; set; }

        public Dictionary<string, string> parameters { get; set; }

        public long elementId { get; set; }

        public string elementCategory { get; set; }
    }
}
