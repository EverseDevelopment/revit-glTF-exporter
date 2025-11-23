namespace Common_glTF_Exporter.Core
{
    using System;

    /// <summary>
    /// Representa el buffer binario completo del GLTF (un solo buffer .bin).
    /// Todos los offsets deben manejarse a través del método Append(...)
    /// para evitar pisar memoria y asegurar alineación a 4 bytes.
    /// </summary>
    public class GLTFBinaryData
    {
        public byte[] byteData { get; private set; } = Array.Empty<byte>();

        /// <summary>
        /// Offset actual del buffer (en bytes).
        /// </summary>
        public int GetCurrentByteOffset() => byteData.Length;

        /// <summary>
        /// Agrega bytes al buffer binario global con padding a 4 bytes.
        /// Devuelve el offset donde comienzan los datos insertados.
        /// </summary>
        public int Append(byte[] data, int alignment = 4)
        {
            if (data == null || data.Length == 0)
                return byteData.Length;

            int offset = byteData.Length;

            // Combinar arrays
            byte[] combined = new byte[offset + data.Length];
            Buffer.BlockCopy(byteData, 0, combined, 0, offset);
            Buffer.BlockCopy(data, 0, combined, offset, data.Length);

            byteData = combined;

            // Padding obligatorio a 4 bytes
            int padding = (alignment - (data.Length % alignment)) % alignment;
            if (padding > 0)
            {
                byte[] temp = byteData;                  
                Array.Resize(ref temp, temp.Length + padding);
                byteData = temp;                           
            }

            return offset;
        }

        /// <summary>
        /// Convierte un array de floats a bytes y los agrega al buffer (4 bytes por float).
        /// </summary>
        public int AppendFloatArray(float[] floats)
        {
            if (floats == null || floats.Length == 0)
                return byteData.Length;

            byte[] data = new byte[floats.Length * sizeof(float)];
            Buffer.BlockCopy(floats, 0, data, 0, data.Length);

            return Append(data); // Usa padding y devuelve offset correcto
        }

        /// <summary>
        /// Convierte un array de ints a bytes y los agrega al buffer (4 bytes por int).
        /// </summary>
        public int AppendIntArray(int[] ints)
        {
            if (ints == null || ints.Length == 0)
                return byteData.Length;

            byte[] data = new byte[ints.Length * sizeof(int)];
            Buffer.BlockCopy(ints, 0, data, 0, data.Length);

            return Append(data); // Usa padding y devuelve offset correcto
        }
    }
}
