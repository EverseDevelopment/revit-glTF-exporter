using System;
using System.Collections.Generic;
using System.IO;
using Common_glTF_Exporter.Utils;

namespace Common_glTF_Exporter.Materials
{
    public static class TextureLocation
    {
        private const string AUTODESK_TEXTURES = @"Autodesk Shared\Materials\Textures\";

        public static List<string> GetPaths()
        {
            var paths = new List<string>
            {
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles),
                    AUTODESK_TEXTURES)
            };

            var externalPaths = RevitIniReader.GetAdditionalRenderAppearancePaths();
            if (externalPaths?.Count > 0)
            {
                paths.AddRange(externalPaths);
            }

            return paths;
        }
    }
}
