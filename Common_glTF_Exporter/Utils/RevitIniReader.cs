using System;
using System.Collections.Generic;
using System.Text;
using Revit_glTF_Exporter;
using System.IO;
using System.Linq;
using Autodesk.Revit.ApplicationServices;

namespace Common_glTF_Exporter.Utils
{
    public static class RevitIniReader
    {
        public static List<string> GetAdditionalRenderAppearancePaths()
        {
            string revitVersion = ExternalApplication.RevitCollectorService.GetApplication().VersionNumber;
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string iniDir = Path.Combine(
                appData,
                "Autodesk",
                "Revit",
                $"Autodesk Revit {revitVersion}"
            );

            string iniPath = Path.Combine(iniDir, "Revit.ini");

            if (!File.Exists(iniPath))
                return null;

            foreach (var line in File.ReadAllLines(iniPath))
            {
                if (line.StartsWith("AdditionalRenderAppearancePaths="))
                {
                    string pathString = line.Substring("AdditionalRenderAppearancePaths=".Length);
                    var paths = pathString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    var absolutePaths = new List<string>();
                    string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                    foreach (var p in paths)
                    {
                        string cleanedPath = p.Trim().Trim('"');

                        // Replace | with ; or split further if needed
                        cleanedPath = cleanedPath.Replace('|', ';');

                        foreach (var subPath in cleanedPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string trimmedPath = subPath.Trim().Trim('"');

                            if (Path.GetInvalidPathChars().Any(c => trimmedPath.Contains(c)))
                            {
                                // Skip or log invalid path
                                Console.WriteLine($"Skipping invalid path: {trimmedPath}");
                                continue;
                            }

                            string fullPath = Path.IsPathRooted(trimmedPath)
                                ? Path.GetFullPath(trimmedPath)
                                : Path.GetFullPath(Path.Combine(userProfile, trimmedPath));

                            absolutePaths.Add(fullPath);
                        }
                    }

                    return absolutePaths;
                }
            }

            return null;
        }
    }
}
