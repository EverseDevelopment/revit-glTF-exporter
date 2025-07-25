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
                        string trimmedPath = p.Trim();

                        // If already rooted, normalize it
                        if (Path.IsPathRooted(trimmedPath))
                        {
                            absolutePaths.Add(Path.GetFullPath(trimmedPath));
                        }
                        else
                        {
                            // Combine with user profile to resolve relative path
                            string fullPath = Path.GetFullPath(Path.Combine(userProfile, trimmedPath));
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
