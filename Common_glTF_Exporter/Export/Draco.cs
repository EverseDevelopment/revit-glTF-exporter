using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Windows.MainWindow;
using dracowrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common_glTF_Exporter.Export
{

    internal sealed class GlbData
    {
        public string Json;
        public byte[] Bin;
        public GlbData(string json, byte[] bin)
        {
            Json = json;
            Bin = bin ?? new byte[0];
        }
    }

    internal static class GltfExtrasPatcher
    {
        public static void PatchExtras(string originalPath, string tempPath)
        {
            if (string.IsNullOrEmpty(originalPath) || string.IsNullOrEmpty(tempPath))
                throw new ArgumentNullException();

            string ext = (Path.GetExtension(originalPath) ?? "").ToLowerInvariant();
            if (ext == ".gltf")
            {
                PatchExtrasGltf(originalPath, tempPath);
            }
            else if (ext == ".glb")
            {
                PatchExtrasGlb(originalPath, tempPath);
            }
        }

        private static void PatchExtrasGltf(string originalGltf, string tempGltf)
        {
            JObject src = JObject.Parse(File.ReadAllText(originalGltf));
            JObject dst = JObject.Parse(File.ReadAllText(tempGltf));

            PatchArrayByIndex(src, dst, "nodes", PatchNodeLike);

            MergeExtensionsUsedAndRequired(src, dst);

            string baseDir = Path.GetDirectoryName(tempGltf);
            string tempBinFileName = Path.GetFileName(tempGltf).Replace(".gltf", ".bin"); // ej: MyModelTemp.bin

            byte[] binBytes = null;
            InlineExternalImagesIntoBin_JObject(dst, baseDir, tempBinFileName, ref binBytes, false, true);

            if (binBytes != null)
            {
                string binPath = Path.Combine(baseDir, tempBinFileName);
                File.WriteAllBytes(binPath, binBytes);
            }

            File.WriteAllText(tempGltf, dst.ToString(Formatting.None));
        }

        private static void PatchExtrasGlb(string originalGlb, string tempGlb)
        {
            GlbData srcGlb = ReadGlb(originalGlb);
            GlbData dstGlb = ReadGlb(tempGlb);

            JObject src = JObject.Parse(srcGlb.Json);
            JObject dst = JObject.Parse(dstGlb.Json);

            PatchArrayByIndex(src, dst, "nodes", PatchNodeLike);


            MergeExtensionsUsedAndRequired(src, dst);

            byte[] glbBin = dstGlb.Bin;
            string baseDir = Path.GetDirectoryName(tempGlb);
            InlineExternalImagesIntoBin_JObject(dst, baseDir, null, ref glbBin, true, true);

            string newJson = dst.ToString(Formatting.None);
            WriteGlb(tempGlb, newJson, glbBin); 
        }

        private static void PatchNodeLike(JObject srcNode, JObject dstNode)
        {
            CopyExtras(srcNode, dstNode);
            CopyUnknownExtensions(srcNode, dstNode, new[] { "KHR_draco_mesh_compression" });
        }

        private static void PatchArrayByIndex(JObject src, JObject dst, string name, Action<JObject, JObject> patchItem)
        {
            JArray sa = src[name] as JArray;
            JArray da = dst[name] as JArray;
            if (sa == null || da == null) return;

            int count = Math.Min(sa.Count, da.Count);
            for (int i = 0; i < count; i++)
                patchItem((JObject)sa[i], (JObject)da[i]);
        }

        private static void CopyExtras(JObject src, JObject dst)
        {
            if (src["extras"] != null)
                dst["extras"] = src["extras"].DeepClone();
        }

        private static void CopyUnknownExtensions(JObject src, JObject dst, IEnumerable<string> keepKnown)
        {
            JObject sExt = src["extensions"] as JObject;
            if (sExt == null) return;

            JObject dExt = dst["extensions"] as JObject;
            if (dExt == null) dExt = new JObject();

            HashSet<string> known = new HashSet<string>(keepKnown ?? new string[0], StringComparer.Ordinal);

            foreach (JProperty prop in sExt.Properties())
            {
                if (known.Contains(prop.Name) && dExt[prop.Name] != null) continue;

                dExt[prop.Name] = prop.Value.DeepClone();
            }

            if (dExt.HasValues)
                dst["extensions"] = dExt;
        }

        private static void MergeExtensionsUsedAndRequired(JObject src, JObject dst)
        {
            JArray srcUsed = src["extensionsUsed"] as JArray;
            if (srcUsed != null && srcUsed.Count > 0)
            {
                JArray dstUsed = dst["extensionsUsed"] as JArray;
                if (dstUsed == null) { dstUsed = new JArray(); dst["extensionsUsed"] = dstUsed; }

                var set = new HashSet<string>(StringComparer.Ordinal);
                foreach (var t in dstUsed) set.Add((string)t);

                foreach (var t in srcUsed)
                {
                    string name = (string)t;
                    if (!set.Contains(name)) { dstUsed.Add(name); set.Add(name); }
                }
            }

            JArray srcReq = src["extensionsRequired"] as JArray;
            if (srcReq != null && srcReq.Count > 0)
            {
                JArray dstReq = dst["extensionsRequired"] as JArray;
                if (dstReq == null) { dstReq = new JArray(); dst["extensionsRequired"] = dstReq; }

                var setR = new HashSet<string>(StringComparer.Ordinal);
                foreach (var t in dstReq) setR.Add((string)t);

                foreach (var t in srcReq)
                {
                    string name = (string)t;
                    if (!setR.Contains(name)) { dstReq.Add(name); setR.Add(name); }
                }
            }

            // asegurar Draco en extensionsUsed
            EnsureExtInArray(dst, "extensionsUsed", "KHR_draco_mesh_compression");
        }

        private static void EnsureExtInArray(JObject dst, string arrayName, string ext)
        {
            JArray arr = dst[arrayName] as JArray;
            if (arr == null) { arr = new JArray(); dst[arrayName] = arr; }
            foreach (var t in arr) if (string.Equals((string)t, ext, StringComparison.Ordinal)) return;
            arr.Add(ext);
        }

        private static void InlineExternalImagesIntoBin_JObject(
            JObject model,
            string baseDir,
            string desiredBinFileName,
            ref byte[] binBytes,
            bool isGlb,
            bool removeExternalImageFiles)
        {
            var buffers = model["buffers"] as JArray;
            if (buffers == null) { buffers = new JArray(); model["buffers"] = buffers; }
            if (buffers.Count == 0) buffers.Add(new JObject { ["byteLength"] = 0 });

            var bufferViews = model["bufferViews"] as JArray;
            if (bufferViews == null) { bufferViews = new JArray(); model["bufferViews"] = bufferViews; }

            var buf0 = (JObject)buffers[0];

            if (!isGlb)
            {
                if (string.IsNullOrEmpty(desiredBinFileName)) desiredBinFileName = "sceneTemp.bin";
                buf0["uri"] = desiredBinFileName;

                string desiredBinPath = Path.Combine(baseDir, desiredBinFileName);
                if ((binBytes == null || binBytes.Length == 0) && File.Exists(desiredBinPath))
                    binBytes = File.ReadAllBytes(desiredBinPath);
            }

            int appendOffset = binBytes != null ? binBytes.Length : 0;
            var images = model["images"] as JArray;
            var consumedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (images != null && images.Count > 0)
            {
                for (int i = 0; i < images.Count; i++)
                {
                    var img = images[i] as JObject;
                    if (img == null) continue;
                    if (img["bufferView"] != null) continue; // ya embebida

                    string uri = (string)img["uri"];
                    if (string.IsNullOrEmpty(uri)) continue;
                    if (uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue; // data URI

                    string imgPath = Path.Combine(baseDir, uri);
                    if (!File.Exists(imgPath)) continue;

                    byte[] imgBytes = File.ReadAllBytes(imgPath);
                    string mime = MimeFromExtension(Path.GetExtension(uri));

                    int thisOffset = appendOffset;
                    binBytes = AppendBytes(binBytes, imgBytes, true, 0x00);
                    int thisLength = imgBytes.Length;
                    appendOffset = binBytes.Length;

                    int bvIndex = bufferViews.Count;
                    var bv = new JObject
                    {
                        ["buffer"] = 0,
                        ["byteOffset"] = thisOffset,
                        ["byteLength"] = thisLength
                    };
                    bufferViews.Add(bv);

                    img.Remove("uri");
                    img["bufferView"] = bvIndex;
                    if (!string.IsNullOrEmpty(mime)) img["mimeType"] = mime;

                    if (removeExternalImageFiles)
                        consumedFiles.Add(imgPath);
                }
            }

            buf0["byteLength"] = binBytes != null ? binBytes.Length : 0;

            // Eliminar los archivos externos que incrustamos
            if (removeExternalImageFiles && consumedFiles.Count > 0)
            {
                foreach (var path in consumedFiles)
                {
                    try { if (File.Exists(path)) File.Delete(path); } catch { /* swallow */ }
                }
            }
        }

        private static string MimeFromExtension(string ext)
        {
            ext = (ext ?? "").ToLowerInvariant();
            if (ext == ".png") return "image/png";
            if (ext == ".jpg" || ext == ".jpeg") return "image/jpeg";
            if (ext == ".ktx2") return "image/ktx2";
            return null;
        }

        private static byte[] AppendBytes(byte[] bin, byte[] add, bool padTo4, byte padByte)
        {
            if (bin == null) bin = new byte[0];
            int oldLen = bin.Length;
            int newLen = oldLen + (add != null ? add.Length : 0);
            byte[] outArr = new byte[newLen];
            if (oldLen > 0) Buffer.BlockCopy(bin, 0, outArr, 0, oldLen);
            if (add != null && add.Length > 0) Buffer.BlockCopy(add, 0, outArr, oldLen, add.Length);

            if (padTo4)
            {
                int mod = outArr.Length % 4;
                if (mod != 0)
                {
                    int pad = 4 - mod;
                    byte[] padded = new byte[outArr.Length + pad];
                    Buffer.BlockCopy(outArr, 0, padded, 0, outArr.Length);
                    for (int i = 0; i < pad; i++) padded[outArr.Length + i] = padByte;
                    return padded;
                }
            }
            return outArr;
        }

        private static GlbData ReadGlb(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (BinaryReader br = new BinaryReader(fs))
            {
                uint magic = br.ReadUInt32();   // 'glTF'
                uint version = br.ReadUInt32(); // 2
                uint length = br.ReadUInt32();

                // chunk 0: JSON
                uint chunkLen0 = br.ReadUInt32();
                uint chunkType0 = br.ReadUInt32();  // 'JSON'
                byte[] jsonBytes = br.ReadBytes((int)chunkLen0);
                string json = System.Text.Encoding.UTF8.GetString(jsonBytes);

                // chunk 1 (opcional): BIN
                byte[] bin = new byte[0];
                if (fs.Position + 8 <= fs.Length)
                {
                    uint chunkLen1 = br.ReadUInt32();
                    uint chunkType1 = br.ReadUInt32(); // 'BIN\0'
                    bin = br.ReadBytes((int)chunkLen1);
                }

                return new GlbData(json, bin);
            }
        }

        private static void WriteGlb(string path, string json, byte[] bin)
        {
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            jsonBytes = PadTo4(jsonBytes, 0x20); // espacios

            byte[] binBytes = (bin ?? new byte[0]);
            binBytes = PadTo4(binBytes, 0x00);

            using (FileStream fs = File.Create(path))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                uint magic = 0x46546C67;   // 'glTF'
                uint version = 2;
                uint length = 12 // header
                               + 8 + (uint)jsonBytes.Length
                               + (binBytes.Length > 0 ? (8 + (uint)binBytes.Length) : 0);

                // header
                bw.Write(magic);
                bw.Write(version);
                bw.Write(length);

                // chunk 0: JSON
                bw.Write((uint)jsonBytes.Length);
                bw.Write(0x4E4F534A); // 'JSON'
                bw.Write(jsonBytes);

                // chunk 1: BIN (si hay)
                if (binBytes.Length > 0)
                {
                    bw.Write((uint)binBytes.Length);
                    bw.Write(0x004E4942); // 'BIN\0'
                    bw.Write(binBytes);
                }
            }
        }

        private static byte[] PadTo4(byte[] data, byte padByte)
        {
            int mod = data.Length % 4;
            if (mod == 0) return data;

            int pad = 4 - mod;
            byte[] outArr = new byte[data.Length + pad];
            Buffer.BlockCopy(data, 0, outArr, 0, data.Length);
            for (int i = 0; i < pad; i++)
                outArr[data.Length + i] = padByte;
            return outArr;
        }
    }
    public static class Draco
    {
        public static void Compress(Preferences preferences)
        {
            List<string> files = new List<string>();
            string fileToCompress;
            string fileToCompressTemp;

            if (preferences.format == FormatEnum.gltf)
            {
                fileToCompress = preferences.path + ".gltf";
                fileToCompressTemp = preferences.path + "Temp.gltf";

                files.Add(preferences.path + ".bin");
                files.Add(fileToCompress);
            }
            else
            {
                fileToCompress = preferences.path + ".glb";
                fileToCompressTemp = preferences.path + "Temp.glb";
                files.Add(fileToCompress);
            }

#if REVIT2025 || REVIT2026
            var loadContext = new NonCollectibleAssemblyLoadContext();
            string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
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

            GltfExtrasPatcher.PatchExtras(fileToCompress, fileToCompressTemp);

            foreach (var x in files)
            {
                try { if (File.Exists(x)) File.Delete(x); } catch { /* ignore */ }
            }
            File_MoveOverwrite(fileToCompressTemp, fileToCompress);

            if (preferences.format == FormatEnum.gltf)
            {
                string binTemp = fileToCompressTemp.Replace(".gltf", ".bin");   
                string binFinal = fileToCompressTemp.Replace("Temp.gltf", ".bin");  

                if (File.Exists(binTemp))
                {
                    File_MoveOverwrite(binTemp, binFinal);
                }


                if (File.Exists(fileToCompress))
                {
                    string text = File.ReadAllText(fileToCompress);
                    string binTempName = Path.GetFileName(binTemp);
                    string binFinalName = Path.GetFileName(binFinal);
                    if (!string.IsNullOrEmpty(binTempName) && !string.IsNullOrEmpty(binFinalName))
                    {
                        text = text.Replace(binTempName, binFinalName);
                        File.WriteAllText(fileToCompress, text);
                    }
                }
            }
        }


        private static void File_MoveOverwrite(string src, string dst)
        {
            if (File.Exists(dst))
            {
                try { File.Delete(dst); } catch { /* ignore */ }
            }
            File.Move(src, dst);
        }
    }
}
