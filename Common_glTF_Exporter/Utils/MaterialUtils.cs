namespace Common_glTF_Exporter.Utils
{
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Windows.MainWindow;
    using Revit_glTF_Exporter;
    using System.Collections.Generic;

    public class MaterialUtils
    {
        public static Material GetMeshMaterial(Document doc, Mesh mesh)
        {
            ElementId materialId = mesh.MaterialElementId;

            if (materialId != null)
            {
                return doc.GetElement(materialId) as Material;
            }
            else
            {
                return null;
            }
        }

        public static GLTFMaterial GetGltfMeshMaterial(Document doc, Preferences preferences, Mesh mesh, IndexedDictionary<GLTFMaterial> materials, bool doubleSided)
        {
            GLTFMaterial gl_mat = new GLTFMaterial();

            Material material = GetMeshMaterial(doc, mesh);

            if (preferences.materials == MaterialsEnum.materials || preferences.materials == MaterialsEnum.textures)
            {
                if (material == null)
                {
                    gl_mat = GLTFExportUtils.GetGLTFMaterial(materials, 1, doubleSided);
                }
                else 
                {
                    gl_mat.doubleSided = doubleSided;
                    float opacity = 1 - (float)material.Transparency;
                    gl_mat.name = material.Name;
                    GLTFPBR pbr = new GLTFPBR();
                    pbr.baseColorFactor = new List<float>(4) { material.Color.Red / 255f, material.Color.Green / 255f, material.Color.Blue / 255f, opacity };
                    pbr.metallicFactor = 0f;
                    pbr.roughnessFactor = 1f;
                    gl_mat.pbrMetallicRoughness = pbr;
                    gl_mat.UniqueId = material.UniqueId;
                }           
            }

            return gl_mat;
        }
    }
}
