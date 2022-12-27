using Autodesk.Revit.DB;
using Revit_glTF_Exporter;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Common_glTF_Exporter.Export
{
    public static class RevitMaterials
    {
        public static void Export(MaterialNode node, Document doc, ref IndexedDictionary<glTFMaterial> Materials)
        {
            ElementId id = node.MaterialId;
            glTFMaterial gl_mat = new glTFMaterial();
            float opacity = 1 - (float)node.Transparency;

            //Validate if the material is valid because for some reason there are
            // materials with invalid Ids
            if (id != ElementId.InvalidElementId)
            {
                // construct a material from the node
                Element m = doc.GetElement(node.MaterialId);
                gl_mat.name = m.Name;
                glTFPBR pbr = new glTFPBR();

                SetMaterialsProperties(node, opacity, ref pbr, ref gl_mat);

                Materials.AddOrUpdateCurrent(m.UniqueId, gl_mat);
            }
        }

        private static void SetMaterialsProperties(MaterialNode node, float opacity, ref glTFPBR pbr, ref glTFMaterial gl_mat)
        {
            pbr.baseColorFactor = new List<float>() { node.Color.Red / 255f, node.Color.Green / 255f, node.Color.Blue / 255f, opacity };
            pbr.metallicFactor = 0f;
            pbr.roughnessFactor = opacity != 1 ? 0.5f : 1f;
            gl_mat.pbrMetallicRoughness = pbr;

            //TODO: Implement MASK alphamode for elements like leaves or wire fences
            gl_mat.alphaMode = opacity != 1 ? "BLEND" : "OPAQUE";
            gl_mat.alphaCutoff = null;
        }
    }
}
