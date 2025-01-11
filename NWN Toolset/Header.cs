using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWN_Toolset
{
    public class Header
    {
        public string GFFType { get; set; }
        public string Version { get; set; }
        public UInt32 StructOffset { get; set; }
        public UInt32 StructCount { get; set; }
        public UInt32 FieldOffset { get; set; }
        public UInt32 FieldCount { get; set; }
        public UInt32 LabelOffset { get; set; }
        public UInt32 LabelCount { get; set; }
        public UInt32 FieldDataOffset { get; set; }
        public UInt32 FieldDataCount { get; set; }
        public UInt32 FieldIndicesOffset { get; set; }
        public UInt32 FieldIndicesCount { get; set; }
        public UInt32 ListIndicesOffset { get; set; }
        public UInt32 ListIndicesCount { get; set; }

        public static Header Read(BinaryReader reader, bool verbose = true)
        {
            reader.BaseStream.Position = 0;

            Header header = new Header
            {
                GFFType = Encoding.UTF8.GetString(reader.ReadBytes(4)),
                Version = Encoding.UTF8.GetString(reader.ReadBytes(4)),
                StructOffset = reader.ReadUInt32(),
                StructCount = reader.ReadUInt32(),
                FieldOffset = reader.ReadUInt32(),
                FieldCount = reader.ReadUInt32(),
                LabelOffset = reader.ReadUInt32(),
                LabelCount = reader.ReadUInt32(),
                FieldDataOffset = reader.ReadUInt32(),
                FieldDataCount = reader.ReadUInt32(),
                FieldIndicesOffset = reader.ReadUInt32(),
                FieldIndicesCount = reader.ReadUInt32(),
                ListIndicesOffset = reader.ReadUInt32(),
                ListIndicesCount = reader.ReadUInt32()
            };

            if(verbose)
                Console.WriteLine($"File Type: {header.GFFType}, Version: {header.Version}");

            return header;
        }
    }
}
