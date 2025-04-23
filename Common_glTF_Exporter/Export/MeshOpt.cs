using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Windows.MainWindow;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using System;

namespace Common_glTF_Exporter.Export
{
    public static class MeshOpt
    {
        public static void Compress(Preferences preferences)
        {
            List<string> files = new List<string>();
            string fileToCompress;
            string fileToCompressTemp;

            if (preferences.format == FormatEnum.gltf)
            {
                fileToCompress = string.Concat(preferences.path, ".gltf");
                fileToCompressTemp = string.Concat(preferences.path, "Temp.gltf");

                files.Add(string.Concat(preferences.path, ".bin"));
                files.Add(fileToCompress);
            }
            else
            {
                fileToCompress = string.Concat(preferences.path, ".glb");
                fileToCompressTemp = string.Concat(preferences.path, "Temp.glb");
                files.Add(fileToCompress);
            }

            #if REVIT2025 || REVIT2026

            var loadContext = new NonCollectibleAssemblyLoadContext();

            string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string meshOptPath = Path.Combine(programDataPath, "Autodesk", "ApplicationPlugins", "leia.bundle", "Contents", "2025", "MeshOpt.dll");
            Assembly mixedModeAssembly = loadContext.LoadFromAssemblyPath(meshOptPath);
            MethodInfo defaultSettings = mixedModeAssembly.GetType("Gltf.GltfSettings").GetMethod("defaults");

            var settings = defaultSettings.Invoke(null, null);
            MethodInfo gltfpack = mixedModeAssembly.GetType("Gltf.GltfPack").GetMethod("gltfpack");
            object[] parameters = new object[4];
            parameters[0] = fileToCompress;
            parameters[1] = fileToCompressTemp;
            parameters[2] = "report.txt";
            parameters[3] = settings;

            gltfpack.Invoke(null, parameters);

#else

            Gltf.GltfSettings settings = Gltf.GltfSettings.defaults();

            Gltf.GltfPack.gltfpack(fileToCompress,
                fileToCompressTemp, "report.txt", settings);

#endif

            if (File.Exists(fileToCompressTemp))
            {
                files.ForEach(x => File.Delete(x));
                File.Move(fileToCompressTemp, fileToCompress);
            }
            else
            {
                Console.WriteLine("The Compression didn't work");
            }
            

            if (preferences.format == FormatEnum.gltf)
            {
                string binToReplace = fileToCompressTemp.Replace(".gltf", ".bin");
                string binFinalName = fileToCompressTemp.Replace("Temp.gltf", ".bin");
                File.Move(binToReplace, binFinalName);

                string text = File.ReadAllText(fileToCompress);
                text = text.Replace(Path.GetFileName(binToReplace), Path.GetFileName(binFinalName));
                File.WriteAllText(fileToCompress, text);
            }
        }
    }
}
