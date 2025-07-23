using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using System;
using System.Collections.Generic;
using System.IO;

namespace Common_glTF_Exporter.Materials
{
    public static class AssetPropertiesUtils
    {
        private static readonly string[] DIFFUSE_NAMES = { 
            Autodesk.Revit.DB.Visual.AdvancedOpaque.OpaqueAlbedo, 
            Autodesk.Revit.DB.Visual.Generic.GenericDiffuse,
            Autodesk.Revit.DB.Visual.AdvancedWood.WoodCurlyDistortionMap,
            Autodesk.Revit.DB.Visual.Hardwood.HardwoodColor,
            Autodesk.Revit.DB.Visual.AdvancedMetal.SurfaceAlbedo          
        };
        private const string AUTODESKPATHTEXTURES = @"Autodesk Shared\Materials\Textures\";

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
                var bitmapPathProp = connectedAsset.FindByName(UnifiedBitmap.UnifiedbitmapBitmap) as AssetPropertyString;

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
                theAsset.FindByName(Generic.GenericDiffuse) as AssetPropertyDoubleArray4d;

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
            AssetPropertyDouble rotation = 
                connectedAsset.FindByName(UnifiedBitmap.TextureWAngle) as AssetPropertyDouble;

            if (rotation != null)
            {
                return (float)(rotation.Value * Math.PI / 180.0);
            }

            return 0f;
        }

        public static float GetScale(Asset connectedAsset, string textureName)
        {
            AssetPropertyDistance scale = 
                connectedAsset.FindByName(textureName) as AssetPropertyDistance;

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

        public static float GetOffset(Asset connectedAsset, string textureName)
        {
            AssetPropertyDistance offset =
                connectedAsset.FindByName(textureName) as AssetPropertyDistance;

            if (offset != null)
            {
                double offsetValue;

#if REVIT2019 || REVIT2020
                offsetValue = UnitUtils.Convert(offset.Value, offset.DisplayUnitType, DisplayUnitType.DUT_DECIMAL_FEET);
#else
            offsetValue = UnitUtils.Convert(offset.Value, offset.GetUnitTypeId(), UnitTypeId.Feet);
#endif

                return (float)offsetValue;
            }

            return 0;
        }

        public static Autodesk.Revit.DB.Color GetTint(Asset asset)
        {
            bool tintOn = true;

            AssetProperty tintEnabledProp = asset.FindByName(Generic.CommonTintToggle);
            if (tintEnabledProp is AssetPropertyBoolean apb)
            {
                tintOn = apb.Value;
            }

            if (tintOn)
            {
                AssetProperty tintProp = asset.FindByName(Generic.CommonTintColor);
                if (tintProp is AssetPropertyDoubleArray4d tintArray4d)
                {
                    IList<double> rgba = tintArray4d.GetValueAsDoubles();

                    byte r = (byte)(rgba[0] * 255.0);
                    byte g = (byte)(rgba[1] * 255.0);
                    byte b = (byte)(rgba[2] * 255.0);

                    return new Autodesk.Revit.DB.Color(r, g, b);
                }
            }

            return null;
        }

        public static double GetFade(Asset asset)
        {
            AssetPropertyDouble fadeProp = asset.FindByName(Generic.GenericDiffuseImageFade) as AssetPropertyDouble;

            if (fadeProp != null)
            {
                return fadeProp.Value;
            }

            return 1;
        }
    }
}
