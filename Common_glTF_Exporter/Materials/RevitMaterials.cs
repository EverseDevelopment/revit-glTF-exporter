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
using glTF.Manipulator.Schema;


namespace Common_glTF_Exporter.Export
{
    public static class RevitMaterials
    {
        const int ONEINTVALUE = 1;

        public static BaseMaterial ProcessMaterial(MaterialNode node,
                Preferences preferences, Document doc, IndexedDictionary<BaseMaterial> materials,
                List<BaseTexture> textures, List<BaseImage> images)
        {
            BaseMaterial material = new BaseMaterial();
            string materialId = node.MaterialId.ToString();
            material.uuid = materialId;

            if (materials.Contains(materialId))
            {
                material = materials.GetElement(materialId);
            }
            else
            {
                Autodesk.Revit.DB.Material revitMaterial = doc.GetElement(node.MaterialId) as Autodesk.Revit.DB.Material;

                if (revitMaterial == null)
                {
                    material = GLTFExportUtils.GetGLTFMaterial(materials, node.Transparency, false);
                }
                else
                {
                    material = RevitMaterials.Export(node, preferences, doc, revitMaterial, textures, images, material);
                }
            }
            materials.AddOrUpdateCurrentMaterial(material.uuid, material, false);

            return material;
        }


        /// <summary>
        /// Export Revit materials.
        /// </summary>
        public static BaseMaterial Export(MaterialNode node,
            Preferences preferences, Document doc, 
            Material revitMaterial, List<BaseTexture> textures,
            List<BaseImage> images, BaseMaterial material)
        {

                float opacity = ONEINTVALUE - (float)node.Transparency;

                material.name = revitMaterial.Name;
                MaterialProperties.SetProperties(node, opacity, ref material);

                (Color, Color) baseNTintColour = (null, null);

                if (revitMaterial != null && preferences.materials == MaterialsEnum.textures)
                {
                    baseNTintColour = MaterialTextures.SetMaterialTextures(revitMaterial, material, doc, opacity, textures, images);
                    material.baseColorFactor = MaterialProperties.GetDefaultColour(opacity);
                }

                if (material.hasTexture)
                {
                    material.baseColorFactor = MaterialProperties.GetDefaultColour(opacity);
                }
                else
                {
                    material.baseColorFactor = MaterialProperties.SetMaterialColour(node, opacity, baseNTintColour.Item1, baseNTintColour.Item2);
                }

            return material;
        }
    }
}

