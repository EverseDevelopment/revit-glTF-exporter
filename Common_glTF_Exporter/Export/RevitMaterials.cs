
namespace Common_glTF_Exporter.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Core;
    using Revit_glTF_Exporter;

    /// <summary>
    /// Revit materials.
    /// </summary>
    public static class RevitMaterials
    {
        /// <summary>
        /// Export Revit materials.
        /// </summary>
        /// <param name="node">node.</param>
        /// <param name="doc">Revit document.</param>
        /// <param name="materials">Materials.</param>
        public static void Export(MaterialNode node, Document doc, ref IndexedDictionary<GLTFMaterial> materials)
        {
            ElementId id = node.MaterialId;
            GLTFMaterial gl_mat = new GLTFMaterial();
            float opacity = 1 - (float)node.Transparency;

            // Validate if the material is valid because for some reason there are
            // materials with invalid Ids
            if (id != ElementId.InvalidElementId)
            {
                // construct a material from the node
                Element m = doc.GetElement(node.MaterialId);
                gl_mat.name = m.Name;
                GLTFPBR pbr = new GLTFPBR();

                SetMaterialsProperties(node, opacity, ref pbr, ref gl_mat);

                materials.AddOrUpdateCurrentMaterial(m.UniqueId, gl_mat, false);
            }
        }

        private static void SetMaterialsProperties(MaterialNode node, float opacity, ref GLTFPBR pbr, ref GLTFMaterial gl_mat)
        {
            pbr.baseColorFactor = new List<float>() { node.Color.Red / 255f, node.Color.Green / 255f, node.Color.Blue / 255f, opacity };
            pbr.metallicFactor = 0f;
            pbr.roughnessFactor = opacity != 1 ? 0.5f : 1f;
            gl_mat.pbrMetallicRoughness = pbr;

            // TODO: Implement MASK alphamode for elements like leaves or wire fences
            gl_mat.alphaMode = opacity != 1 ? "BLEND" : "OPAQUE";
            gl_mat.alphaCutoff = null;
        }
    }
}
