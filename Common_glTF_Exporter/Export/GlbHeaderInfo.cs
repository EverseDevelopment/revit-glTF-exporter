using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common_glTF_Exporter.Model;

namespace Common_glTF_Exporter.Export
{
    public static class GlbHeaderInfo
    {
        public static byte[] Get(byte[] json, byte[] bin)
        {
            GlbHeader glbHeader = new GlbHeader();
            glbHeader.Length = BitConverter.GetBytes(Convert.ToUInt32(json.Length + bin.Length + 12));

            byte[] result = new byte[] { };
            result = result.Concat(glbHeader.Magic()).ToArray();
            result = result.Concat(glbHeader.Version()).ToArray();
            result = result.Concat(glbHeader.Length).ToArray();

            return result;
        }
    }
}
