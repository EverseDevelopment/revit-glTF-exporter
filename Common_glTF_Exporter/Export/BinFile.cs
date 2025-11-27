using System.IO;
using glTF.Manipulator.GenericSchema;

public static class BinFile
{
    /// <summary>
    /// Create a new .bin file using the optimized GLTFBinaryData.
    /// </summary>
    public static void Create(string filename, GLTFBinaryData globalBuffer)
    {
        // Obtiene los bytes finales del MemoryStream
        byte[] data = globalBuffer.ToArray();

        using (FileStream f = File.Create(filename))
        using (var writer = new BinaryWriter(new BufferedStream(f)))
        {
            writer.Write(data);
            writer.Flush();
        }
    }
}
