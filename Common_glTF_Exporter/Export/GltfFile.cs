namespace Common_glTF_Exporter.Export
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Windows.MainWindow;
    using Newtonsoft.Json;
    using Revit_glTF_Exporter;

    public static class GltfFile
    {
        public static void Create(Preferences preferences, string serializedModel)
        {
            string gltfName = string.Concat(preferences.path, ".gltf");
            File.WriteAllText(gltfName, serializedModel);
        }
    }
}
