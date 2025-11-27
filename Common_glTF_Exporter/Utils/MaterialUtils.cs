namespace Common_glTF_Exporter.Utils
{
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Model;
    using Common_glTF_Exporter.Windows.MainWindow;
    using glTF.Manipulator.GenericSchema;
    using glTF.Manipulator.Schema;
    using glTF.Manipulator.Utils;
    using Revit_glTF_Exporter;
    using System.Collections.Generic;

    public class MaterialUtils
    {
        public static Autodesk.Revit.DB.Material GetMeshMaterial(Document doc, Autodesk.Revit.DB.Mesh mesh)
        {
            ElementId materialId = mesh.MaterialElementId;

            if (materialId != null)
            {
                return doc.GetElement(materialId) as Autodesk.Revit.DB.Material;
            }
            else
            {
                return null;
            }
        }

        public static BaseMaterial GetGltfMeshMaterial(Document doc, Preferences preferences, Autodesk.Revit.DB.Mesh mesh, IndexedDictionary<BaseMaterial> materials, bool doubleSided)
        {
            BaseMaterial gl_mat = new BaseMaterial();

            Autodesk.Revit.DB.Material material = GetMeshMaterial(doc, mesh);

            if (preferences.materials == MaterialsEnum.materials || preferences.materials == MaterialsEnum.textures)
            {
                if (material == null)
                {
                    gl_mat = GLTFExportUtils.GetGLTFMaterial(materials);
                }
                else 
                {
                    gl_mat.doubleSided = doubleSided;
                    float opacity = 1 - (float)material.Transparency;
                    gl_mat.name = material.Name;

                    gl_mat.baseColorFactor = new List<float>(4) { material.Color.Red / 255f, material.Color.Green / 255f, material.Color.Blue / 255f, opacity };
                    gl_mat.metallicFactor = 0f;
                    gl_mat.roughnessFactor = 1f;
                }           
            }

            return gl_mat;
        }
    }
}
