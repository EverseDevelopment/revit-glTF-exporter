namespace Common_glTF_Exporter.Export
{
    using System.Collections.Generic;
    using System.Windows;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Common_glTF_Exporter.Core;
    using Revit_glTF_Exporter;

    public static class RevitMaterials
    {
        private static Element element;
        private static GLTFMaterial glTFMaterial;
        private static GLTFPBR pbr;

        /// <summary>
        /// Export Revit materials.
        /// </summary>
        /// <param name="node">node.</param>
        /// <param name="doc">Revit document.</param>
        /// <param name="materials">Materials.</param>
        public static void Export(MaterialNode node, Document doc, ref IndexedDictionary<GLTFMaterial> materials)
        {
            ElementId id = node.MaterialId;
            glTFMaterial = new GLTFMaterial();
            float opacity = 1 - (float)node.Transparency;

            // Validate if the material is valid because for some reason there are
            // materials with invalid Ids
            if (id != ElementId.InvalidElementId)
            {
                // construct a material from the node
                element = doc.GetElement(id);
                glTFMaterial.name = element.Name;
                pbr = new GLTFPBR();

                SetMaterialsProperties(node, opacity, ref pbr, ref glTFMaterial);

                materials.AddOrUpdateCurrentMaterial(element.UniqueId, glTFMaterial, false);
            }
            else
            {
                glTFMaterial.name = "Default";
                pbr = new GLTFPBR();
                SetMaterialsProperties(node, opacity, ref pbr, ref glTFMaterial);
                materials.AddOrUpdateCurrentMaterial("26a40dc9-494e-44c0-8c3b-611fa79ba86d", glTFMaterial, false);
            }
        }

        private static void SetMaterialsProperties(MaterialNode node, float opacity, ref GLTFPBR pbr, ref GLTFMaterial glTFMaterial)
        {
            pbr.baseColorFactor = new List<float>() { node.Color.Red / 255f, node.Color.Green / 255f, node.Color.Blue / 255f, opacity };
            pbr.metallicFactor = 0f;
            pbr.roughnessFactor = opacity != 1 ? 0.5f : 1f;
            glTFMaterial.pbrMetallicRoughness = pbr;

            // TODO: Implement MASK alphamode for elements like leaves or wire fences
            glTFMaterial.alphaMode = opacity != 1 ? "BLEND" : "OPAQUE";
            glTFMaterial.alphaCutoff = null;
        }
    }
}
