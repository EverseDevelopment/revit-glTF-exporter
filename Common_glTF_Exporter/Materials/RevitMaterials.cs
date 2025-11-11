using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Windows.MainWindow;
using Revit_glTF_Exporter;
using Common_glTF_Exporter.Materials;
using Common_glTF_Exporter.Model;
using System.IO.Ports;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Material = Autodesk.Revit.DB.Material;
using Common_glTF_Exporter.Utils;


namespace Common_glTF_Exporter.Export
{
    public static class RevitMaterials
    {
        const int ONEINTVALUE = 1;

        public static GLTFMaterial ProcessMaterial(MaterialNode node,
                Preferences preferences, Document doc, IndexedDictionary<GLTFMaterial> materials)
        {
            GLTFMaterial gl_mat;

            string materialId = node.MaterialId.ToString();
            if (materials.Contains(materialId))
            {
                gl_mat = materials.GetElement(materialId);
            }
            else
            {
                Autodesk.Revit.DB.Material material = doc.GetElement(node.MaterialId) as Autodesk.Revit.DB.Material;

                if (material == null)
                {
                    gl_mat = GLTFExportUtils.GetGLTFMaterial(materials, node.Transparency, false);
                    materialId = gl_mat.UniqueId;
                }
                else
                {
                    gl_mat = RevitMaterials.Export(node, preferences, doc, material);
                }
            }
            materials.AddOrUpdateCurrentMaterial(materialId, gl_mat, false);

            return gl_mat;
        }


        /// <summary>
        /// Export Revit materials.
        /// </summary>
        public static GLTFMaterial Export(MaterialNode node,
            Preferences preferences, Document doc, Material material)
        {

                GLTFMaterial gl_mat = new GLTFMaterial();
                float opacity = ONEINTVALUE - (float)node.Transparency;

                gl_mat.name = material.Name;
                gl_mat.UniqueId = node.MaterialId.ToString();

                GLTFPBR pbr = new GLTFPBR();
                MaterialProperties.SetProperties(node, opacity, ref pbr, ref gl_mat);

                if (material != null && preferences.materials == MaterialsEnum.textures)
                {
                    MaterialTextures.SetMaterialTextures(material, gl_mat, doc, opacity);
                }

                MaterialProperties.SetMaterialColour(node, opacity, ref pbr, ref gl_mat);


            return gl_mat;
        }
    }
}

