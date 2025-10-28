using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using System;
using System.Collections.Generic;
using System.IO;
using Revit_glTF_Exporter;

namespace Common_glTF_Exporter.Materials
{
    public static class AssetPropertiesUtils
    {
        private static readonly Dictionary<string, string> DiffusePropertyMap = new Dictionary<string, string>
        {
            { "ConcreteSchema", Autodesk.Revit.DB.Visual.Concrete.ConcreteBmMap },
            { "PrismGenericSchema", Autodesk.Revit.DB.Visual.Generic.GenericDiffuse },
            { "GenericSchema", Autodesk.Revit.DB.Visual.Generic.GenericDiffuse },
            { "PrismMetalSchema", Autodesk.Revit.DB.Visual.AdvancedMetal.SurfaceAlbedo },
            { "PrismWoodSchema", Autodesk.Revit.DB.Visual.AdvancedWood.WoodCurlyDistortionMap },
            { "HardwoodSchema", Autodesk.Revit.DB.Visual.Hardwood.HardwoodColor },
            { "PrismMasonryCMUSchema", Autodesk.Revit.DB.Visual.MasonryCMU.MasonryCMUColor },
            { "MasonryCMUSchema", Autodesk.Revit.DB.Visual.MasonryCMU.MasonryCMUColor },
            { "PrismOpaqueSchema", Autodesk.Revit.DB.Visual.AdvancedOpaque.OpaqueAlbedo }
        };

        public static Asset GetDiffuseBitmap(Asset theAsset, string baseSchema)
        {
            if (theAsset == null || string.IsNullOrEmpty(baseSchema))
                return null;

            if (!DiffusePropertyMap.TryGetValue(baseSchema, out string diffusePropertyName))
                return null;

            var prop = theAsset.FindByName(diffusePropertyName);

            if (prop == null) 
                return null;

            if (prop.NumberOfConnectedProperties == 1)
            {
                return prop.GetSingleConnectedAsset();
            }

            return null;
        }

        private static readonly Dictionary<string, string> ColorPropertyMap = new Dictionary<string, string>
        {
            { "PrismGenericSchema", Generic.GenericDiffuse },
            { "ConcreteSchema", Concrete.ConcreteColor },
            { "WallPaintSchema", WallPaint.WallpaintColor },
            { "PlasticVinylSchema", PlasticVinyl.PlasticvinylColor },
            { "MetallicPaintSchema", MetallicPaint.MetallicpaintBaseColor },
            { "CeramicSchema", Ceramic.CeramicColor },
            { "MetalSchema", Metal.MetalColor },
            { "PrismMetalSchema", AdvancedMetal.MetalF0 },
            { "PrismOpaqueSchema", AdvancedOpaque.OpaqueAlbedo },
            { "PrismLayeredSchema", AdvancedLayered.LayeredDiffuse },
            { "GenericSchema", Generic.GenericDiffuse },     
            { "AdvancedMetalSchema", AdvancedMetal.SurfaceAlbedo }
        };

        public static Autodesk.Revit.DB.Color GetAppearanceColor(Asset theAsset, string baseSchema)
        {
            if (theAsset == null || string.IsNullOrEmpty(baseSchema))
                return null;

            if (!ColorPropertyMap.TryGetValue(baseSchema, out string colorPropertyName))
                return null;

            var colorProperty = theAsset.FindByName(colorPropertyName) as AssetPropertyDoubleArray4d;

            if (colorProperty != null)
            {
                IList<double> colorValues = colorProperty.GetValueAsDoubles();

                return new Autodesk.Revit.DB.Color(
                    (byte)(colorValues[0] * 255.0),
                    (byte)(colorValues[1] * 255.0),
                    (byte)(colorValues[2] * 255.0)
                );
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
                    string relativeOrAbsolutePath = bitmapPathProp.Value.Split('|')[0].Replace("/", "\\");

                    // If already absolute and the file exists, return it directly
                    if (Path.IsPathRooted(relativeOrAbsolutePath) && File.Exists(relativeOrAbsolutePath))
                    {
                        return relativeOrAbsolutePath;
                    }

                    // Otherwise, search each base path
                    foreach (string basePath in MainWindow.TexturePaths)
                    {
                        string candidatePath = Path.Combine(basePath, relativeOrAbsolutePath);
                        string fullPath = Path.GetFullPath(candidatePath);

                        if (File.Exists(fullPath))
                        {
                            return fullPath;
                        }
                    }
                }
            }

            return null;
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
