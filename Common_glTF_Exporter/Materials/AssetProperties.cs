using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using System;
using System.IO;

namespace Common_glTF_Exporter.Materials
{
    public static class AssetPropertiesUtils
    {
        private static readonly string[] DIFFUSE_NAMES = { "opaque_albedo", "generic_diffuse" };
        private const string PATHPROPERTY = "unifiedbitmap_Bitmap";
        private const string AUTODESKPATHTEXTURES = @"Autodesk Shared\Materials\Textures\";
        private const string ROTATIONPROPERTY = "texture_WAngle";
        private const string DIFUSSEDEFINITION = "generic_diffuse";

        public static Asset GetDiffuseBitmap(Asset theAsset)
        {
            foreach (var name in DIFFUSE_NAMES)
            {
                var prop = theAsset.FindByName(name);
                if (prop?.NumberOfConnectedProperties > 0)
                {
                    var connected = prop.GetSingleConnectedAsset();
                    if (connected != null)
                        return connected;
                }
            }

            return null;
        }

        public static string GetTexturePath(Asset connectedAsset)
        {
            if (connectedAsset != null)
            {
                var bitmapPathProp = connectedAsset.FindByName(PATHPROPERTY) as AssetPropertyString;

                if (bitmapPathProp != null && !string.IsNullOrEmpty(bitmapPathProp.Value))
                {
                    string texturePath = bitmapPathProp.Value.Split('|')[0].Replace("/", "\\");

                    if (!Path.IsPathRooted(texturePath))
                    {
                        string materialsPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles),
                            AUTODESKPATHTEXTURES);
                        texturePath = Path.Combine(materialsPath, texturePath);
                    }

                    return texturePath;
                }
            }

            return null;
        }

        public static Autodesk.Revit.DB.Color GetAppearenceColor(Asset theAsset)
        {
            Autodesk.Revit.DB.Color appearenceColor = Autodesk.Revit.DB.Color.InvalidColorValue;

            AssetPropertyDoubleArray4d colorProperty =
                theAsset.FindByName(DIFUSSEDEFINITION) as AssetPropertyDoubleArray4d;

            if (colorProperty != null)
            {
                var colour = colorProperty.GetValueAsDoubles();
                appearenceColor = new Autodesk.Revit.DB.Color(
                    (byte)(colour[0] * 255.0),
                    (byte)(colour[1] * 255.0),
                    (byte)(colour[2] * 255.0));
            }

            return appearenceColor;
        }

        public static float GetRotationRadians(Asset connectedAsset)
        {
            AssetPropertyDouble rotation = connectedAsset.FindByName(ROTATIONPROPERTY) as AssetPropertyDouble;

            if (rotation != null)
            {
                return (float)(rotation.Value * Math.PI / 180.0);
            }

            return 0f;
        }

        public static float GetScale(Asset connectedAsset, string textureName)
        {
            AssetPropertyDistance scale = connectedAsset.FindByName(textureName) as AssetPropertyDistance;

            if (scale != null)
            {
                double scaledValue;
                #if REVIT2019 || REVIT2020
                    scaledValue = UnitUtils.Convert(scale.Value, scale.DisplayUnitType, DisplayUnitType.DUT_DECIMAL_FEET);
                #else
                scaledValue = UnitUtils.Convert(scale.Value, scale.GetUnitTypeId(), UnitTypeId.Feet);
                #endif

                return (float)scaledValue;
            }

            return 1;
        }
    }
}
