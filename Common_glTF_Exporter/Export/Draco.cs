using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Windows.MainWindow;
using dracowrapper;

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

        private static JsonNode DeepClone(JsonNode node)
        {
            if (node == null) return null;

            string json = node.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
            return JsonNode.Parse(json);
        }

        public static void PatchExtras(string originalPath, string tempPath)
        {
            if (string.IsNullOrEmpty(originalPath) || string.IsNullOrEmpty(tempPath))
                throw new ArgumentNullException();

            string ext = (Path.GetExtension(originalPath) ?? string.Empty).ToLowerInvariant();
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
            JsonObject src = JsonNode.Parse(File.ReadAllText(originalGltf)).AsObject();
            JsonObject dst = JsonNode.Parse(File.ReadAllText(tempGltf)).AsObject();

            PatchArrayByIndex(src, dst, "nodes", PatchNodeLike);

            MergeExtensionsUsedAndRequired(src, dst);

            string baseDir = Path.GetDirectoryName(tempGltf);
            string tempBinFileName = Path.GetFileName(tempGltf).Replace(".gltf", ".bin"); 

            byte[] binBytes = null;
            InlineExternalImagesIntoBin_JObject(dst, baseDir, tempBinFileName, ref binBytes, false, true);

            if (binBytes != null)
            {
                string binPath = Path.Combine(baseDir, tempBinFileName);
                File.WriteAllBytes(binPath, binBytes);
            }

            string jsonOut = dst.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
            File.WriteAllText(tempGltf, jsonOut);
        }

        private static void PatchExtrasGlb(string originalGlb, string tempGlb)
        {
            GlbData srcGlb = ReadGlb(originalGlb);
            GlbData dstGlb = ReadGlb(tempGlb);

            JsonObject src = JsonNode.Parse(srcGlb.Json).AsObject();
            JsonObject dst = JsonNode.Parse(dstGlb.Json).AsObject();

            PatchArrayByIndex(src, dst, "nodes", PatchNodeLike);

            MergeExtensionsUsedAndRequired(src, dst);

            byte[] glbBin = dstGlb.Bin;
            string baseDir = Path.GetDirectoryName(tempGlb);
            InlineExternalImagesIntoBin_JObject(dst, baseDir, null, ref glbBin, true, true);

            string newJson = dst.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
            WriteGlb(tempGlb, newJson, glbBin);
        }

        private static void PatchNodeLike(JsonObject srcNode, JsonObject dstNode)
        {
            CopyExtras(srcNode, dstNode);
            CopyUnknownExtensions(srcNode, dstNode, new[] { "KHR_draco_mesh_compression" });
        }

        private static void PatchArrayByIndex(JsonObject src, JsonObject dst, string name, Action<JsonObject, JsonObject> patchItem)
        {
            JsonNode saNode = src[name];
            JsonNode daNode = dst[name];

            JsonArray sa = saNode as JsonArray;
            JsonArray da = daNode as JsonArray;

            if (sa == null || da == null) return;

            int count = Math.Min(sa.Count, da.Count);
            for (int i = 0; i < count; i++)
            {
                JsonObject sItem = sa[i] as JsonObject;
                JsonObject dItem = da[i] as JsonObject;
                if (sItem == null || dItem == null) continue;

                patchItem(sItem, dItem);
            }
        }

        private static void CopyExtras(JsonObject src, JsonObject dst)
        {
            JsonNode extras;
            if (src.TryGetPropertyValue("extras", out extras) && extras != null)
            {
                dst["extras"] = DeepClone(extras);
            }
        }

        private static void CopyUnknownExtensions(JsonObject src, JsonObject dst, IEnumerable<string> keepKnown)
        {
            JsonNode sExtNode;
            if (!src.TryGetPropertyValue("extensions", out sExtNode)) return;

            JsonObject sExt = sExtNode as JsonObject;
            if (sExt == null) return;

            JsonNode dExtNode;
            JsonObject dExt = null;
            if (dst.TryGetPropertyValue("extensions", out dExtNode) && dExtNode is JsonObject)
            {
                dExt = (JsonObject)dExtNode;
            }
            else
            {
                dExt = new JsonObject();
            }

            HashSet<string> known = new HashSet<string>(keepKnown ?? new string[0], StringComparer.Ordinal);

            foreach (KeyValuePair<string, JsonNode> kvp in sExt)
            {
                string name = kvp.Key;
                JsonNode value = kvp.Value;
                if (value == null) continue;

                if (known.Contains(name) && dExt.ContainsKey(name)) continue;

                dExt[name] = DeepClone(value);
            }

            if (dExt.Count > 0)
                dst["extensions"] = dExt;
        }

        private static void MergeExtensionsUsedAndRequired(JsonObject src, JsonObject dst)
        {
            JsonArray srcUsed = null;
            JsonNode srcUsedNode;
            if (src.TryGetPropertyValue("extensionsUsed", out srcUsedNode))
                srcUsed = srcUsedNode as JsonArray;

            if (srcUsed != null && srcUsed.Count > 0)
            {
                JsonArray dstUsed = null;
                JsonNode dstUsedNode;
                if (dst.TryGetPropertyValue("extensionsUsed", out dstUsedNode) && dstUsedNode is JsonArray)
                {
                    dstUsed = (JsonArray)dstUsedNode;
                }
                else
                {
                    dstUsed = new JsonArray();
                    dst["extensionsUsed"] = dstUsed;
                }

                HashSet<string> set = new HashSet<string>(StringComparer.Ordinal);
                foreach (JsonNode t in dstUsed)
                {
                    if (t == null) continue;
                    set.Add(t.GetValue<string>());
                }

                foreach (JsonNode t in srcUsed)
                {
                    if (t == null) continue;
                    string name = t.GetValue<string>();
                    if (!set.Contains(name))
                    {
                        dstUsed.Add(name);
                        set.Add(name);
                    }
                }
            }

            JsonArray srcReq = null;
            JsonNode srcReqNode;
            if (src.TryGetPropertyValue("extensionsRequired", out srcReqNode))
                srcReq = srcReqNode as JsonArray;

            if (srcReq != null && srcReq.Count > 0)
            {
                JsonArray dstReq = null;
                JsonNode dstReqNode;
                if (dst.TryGetPropertyValue("extensionsRequired", out dstReqNode) && dstReqNode is JsonArray)
                {
                    dstReq = (JsonArray)dstReqNode;
                }
                else
                {
                    dstReq = new JsonArray();
                    dst["extensionsRequired"] = dstReq;
                }

                HashSet<string> setR = new HashSet<string>(StringComparer.Ordinal);
                foreach (JsonNode t in dstReq)
                {
                    if (t == null) continue;
                    setR.Add(t.GetValue<string>());
                }

                foreach (JsonNode t in srcReq)
                {
                    if (t == null) continue;
                    string name = t.GetValue<string>();
                    if (!setR.Contains(name))
                    {
                        dstReq.Add(name);
                        setR.Add(name);
                    }
                }
            }

            EnsureExtInArray(dst, "extensionsUsed", "KHR_draco_mesh_compression");
        }

        private static void EnsureExtInArray(JsonObject dst, string arrayName, string ext)
        {
            JsonArray arr = null;
            JsonNode arrNode;

            if (dst.TryGetPropertyValue(arrayName, out arrNode) && arrNode is JsonArray)
            {
                arr = (JsonArray)arrNode;
            }
            else
            {
                arr = new JsonArray();
                dst[arrayName] = arr;
            }

            foreach (JsonNode t in arr)
            {
                if (t == null) continue;
                if (string.Equals(t.GetValue<string>(), ext, StringComparison.Ordinal))
                    return;
            }

            arr.Add(ext);
        }

        private static void InlineExternalImagesIntoBin_JObject(
            JsonObject model,
            string baseDir,
            string desiredBinFileName,
            ref byte[] binBytes,
            bool isGlb,
            bool removeExternalImageFiles)
        {
            // buffers
            JsonArray buffers = model["buffers"] as JsonArray;
            if (buffers == null)
            {
                buffers = new JsonArray();
                model["buffers"] = buffers;
            }
            if (buffers.Count == 0)
            {
                JsonObject bufObj = new JsonObject();
                bufObj["byteLength"] = 0;
                buffers.Add(bufObj);
            }

            // bufferViews
            JsonArray bufferViews = model["bufferViews"] as JsonArray;
            if (bufferViews == null)
            {
                bufferViews = new JsonArray();
                model["bufferViews"] = bufferViews;
            }

            JsonObject buf0 = buffers[0] as JsonObject;
            if (buf0 == null)
            {
                buf0 = new JsonObject();
                buffers[0] = buf0;
            }

            if (!isGlb)
            {
                if (string.IsNullOrEmpty(desiredBinFileName)) desiredBinFileName = "sceneTemp.bin";
                buf0["uri"] = desiredBinFileName;

                string desiredBinPath = Path.Combine(baseDir, desiredBinFileName);
                if ((binBytes == null || binBytes.Length == 0) && File.Exists(desiredBinPath))
                    binBytes = File.ReadAllBytes(desiredBinPath);
            }

            int appendOffset = binBytes != null ? binBytes.Length : 0;
            JsonArray images = model["images"] as JsonArray;
            HashSet<string> consumedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (images != null && images.Count > 0)
            {
                for (int i = 0; i < images.Count; i++)
                {
                    JsonObject img = images[i] as JsonObject;
                    if (img == null) continue;

                    if (img["bufferView"] != null) continue;

                    JsonNode uriNode = img["uri"];
                    string uri = uriNode != null ? uriNode.GetValue<string>() : null;
                    if (string.IsNullOrEmpty(uri)) continue;
                    if (uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue; 

                    string imgPath = Path.Combine(baseDir, uri);
                    if (!File.Exists(imgPath)) continue;

                    byte[] imgBytes = File.ReadAllBytes(imgPath);
                    string mime = MimeFromExtension(Path.GetExtension(uri));

                    int thisOffset = appendOffset;
                    binBytes = AppendBytes(binBytes, imgBytes, true, 0x00);
                    int thisLength = imgBytes.Length;
                    appendOffset = binBytes.Length;

                    int bvIndex = bufferViews.Count;
                    JsonObject bv = new JsonObject();
                    bv["buffer"] = 0;
                    bv["byteOffset"] = thisOffset;
                    bv["byteLength"] = thisLength;
                    bufferViews.Add(bv);

                    img.Remove("uri");
                    img["bufferView"] = bvIndex;
                    if (!string.IsNullOrEmpty(mime))
                        img["mimeType"] = mime;

                    if (removeExternalImageFiles)
                        consumedFiles.Add(imgPath);
                }
            }

            buf0["byteLength"] = binBytes != null ? binBytes.Length : 0;

            if (removeExternalImageFiles && consumedFiles.Count > 0)
            {
                foreach (string path in consumedFiles)
                {
                    try
                    {
                        if (File.Exists(path)) File.Delete(path);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static string MimeFromExtension(string ext)
        {
            ext = (ext ?? string.Empty).ToLowerInvariant();
            if (ext == ".png") return "image/png";
            if (ext == ".jpg" || ext == ".jpeg") return "image/jpeg";
            if (ext == ".ktx2") return "image/ktx2";
            return null;
        }

        private static byte[] AppendBytes(byte[] bin, byte[] add, bool padTo4, byte padByte)
        {
            if (bin == null) bin = new byte[0];
            int oldLen = bin.Length;
            int addLen = (add != null ? add.Length : 0);
            int newLen = oldLen + addLen;

            byte[] outArr = new byte[newLen];
            if (oldLen > 0)
                Buffer.BlockCopy(bin, 0, outArr, 0, oldLen);
            if (add != null && add.Length > 0)
                Buffer.BlockCopy(add, 0, outArr, oldLen, add.Length);

            if (padTo4)
            {
                int mod = outArr.Length % 4;
                if (mod != 0)
                {
                    int pad = 4 - mod;
                    byte[] padded = new byte[outArr.Length + pad];
                    Buffer.BlockCopy(outArr, 0, padded, 0, outArr.Length);
                    for (int i = 0; i < pad; i++)
                        padded[outArr.Length + i] = padByte;
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
            jsonBytes = PadTo4(jsonBytes, 0x20);

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

            foreach (string x in files)
            {
                try
                {
                    if (File.Exists(x)) File.Delete(x);
                }
                catch
                {
                    // ignore
                }
            }
            File_MoveOverwrite(fileToCompressTemp, fileToCompress);

            if (preferences.format == FormatEnum.gltf)
            {
                string binTemp = fileToCompressTemp.Replace(".gltf", ".bin");
                string binFinal = preferences.path + ".bin";

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
                try { File.Delete(dst); }
                catch { /* ignore */ }
            }
            File.Move(src, dst);
        }
    }
}
