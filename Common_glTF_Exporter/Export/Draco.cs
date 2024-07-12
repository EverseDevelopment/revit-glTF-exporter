using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Windows.MainWindow;
using dracowrapper;

namespace Common_glTF_Exporter.Export
{
    public static class Draco
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

            #if REVIT2025

            var loadContext = new NonCollectibleAssemblyLoadContext();

            string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string assemblyPath = Path.Combine(programDataPath, "Autodesk", "ApplicationPlugins", "leia.bundle", "Contents", "2025", "DracoWrapper.dll");

            Assembly mixedModeAssembly = loadContext.LoadFromAssemblyPath(assemblyPath);

            var gltfDecoderType = mixedModeAssembly.GetType("dracowrapper.GltfDecoder");
            var gltfDecoderInstance = Activator.CreateInstance(gltfDecoderType);

            var decodeFromFileToSceneMethod = gltfDecoderType.GetMethod("DecodeFromFileToScene");
            var res = decodeFromFileToSceneMethod.Invoke(gltfDecoderInstance, new object[] { fileToCompress });
            var resType = res.GetType();
            var valueMethod = resType.GetMethod("Value");
            var scene = valueMethod.Invoke(res, null);

            var dracoCompressionOptionsType = mixedModeAssembly.GetType("dracowrapper.DracoCompressionOptions");
            var dracoCompressionOptionsInstance = Activator.CreateInstance(dracoCompressionOptionsType);

            var sceneUtilsType = mixedModeAssembly.GetType("dracowrapper.SceneUtils");
            var setDracoCompressionOptionsMethod = sceneUtilsType.GetMethod("SetDracoCompressionOptions");
            setDracoCompressionOptionsMethod.Invoke(null, new object[] { dracoCompressionOptionsInstance, scene });

            var gltfEncoderType = mixedModeAssembly.GetType("dracowrapper.GltfEncoder");
            var gltfEncoderInstance = Activator.CreateInstance(gltfEncoderType);
            var encodeSceneToFileMethod = gltfEncoderType.GetMethod("EncodeSceneToFile");
            encodeSceneToFileMethod.Invoke(gltfEncoderInstance, new object[] { scene, fileToCompressTemp });

            #else

            var decoder = new GltfDecoder();
            var res = decoder.DecodeFromFileToScene(fileToCompress);
            var scene = res.Value();
            DracoCompressionOptions options = new DracoCompressionOptions();
            SceneUtils.SetDracoCompressionOptions(options, scene);
            var encoder = new GltfEncoder();
            encoder.EncodeSceneToFile(scene, fileToCompressTemp);

            #endif

            files.ForEach(x => File.Delete(x));
            File.Move(fileToCompressTemp, fileToCompress);

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
