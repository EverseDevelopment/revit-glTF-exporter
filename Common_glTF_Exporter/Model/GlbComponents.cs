using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Model
{
    public class GlbHeader
    {
        public byte[] Magic()
        {
            return new byte[] { 0x67, 0x6C, 0x54, 0x46, };
        }

        public byte[] Version()
        {
            return new byte[] { 0x02, 0x00, 0x00, 0x00, };
        }

        public byte[] Length { get; set; }

    }

    public class GlbJson
    {
        public byte[] Length { get; set; }

        public byte[] ChunkType()
        {
            return new byte[] { 0x4a, 0x53, 0x4f, 0x4e, };
        }

        public byte[] ChunkData { get; set; }

    }

    public class GlbBin
    {
        public byte[] Length { get; set; }

        public byte[] ChunkType()
        {
            return new byte[] { 0x42, 0x49, 0x4e, 0x00, };
        }

        public byte[] ChunkData { get; set; }

    }
}
