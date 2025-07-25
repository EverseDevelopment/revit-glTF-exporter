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


namespace Common_glTF_Exporter.Export
{
    public static class RevitMaterials
    {
        const int ONEINTVALUE = 1;

        /// <summary>
        /// Export Revit materials.
        /// </summary>
        public static GLTFMaterial Export(MaterialNode node,
            Preferences preferences, Document doc)
        {
            GLTFMaterial gl_mat = new GLTFMaterial();
            float opacity = ONEINTVALUE - (float)node.Transparency;

            Material material = doc.GetElement(node.MaterialId) as Material;

                if (material == null)
                {
                    return gl_mat;
                }

                gl_mat.name = material.Name;
                gl_mat.UniqueId = material.UniqueId;
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

