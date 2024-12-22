using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NWN_Toolset
{
    class Program
    {
        static void Main(string[] args)
        {
            // string filePath = "C:\\Users\\joemc\\Documents\\Neverwinter Nights\\modules\\temp0\\warham_crusher20.uti";
            string filePath = "C:\\Users\\joemc\\Documents\\Neverwinter Nights\\modules\\temp0\\brakiraseye.uti";
            GFFFile file = GFFReader.ReadGFFFile(filePath);
            Console.WriteLine($"Done reading GFF file: {filePath}");
        }
    }

    public class GFFReader
    {
        public static GFFFile ReadGFFFile(string filePath)
        {
            GFFFile file = new GFFFile();
            try
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
                {
                    Header header = Header.Read(reader);
                    List<Struct> structs = Struct.Read(reader, header);
                    List<string> labels = Label.Read(reader, header);
                    List<Field> fields = Field.Read(reader, header, labels, structs);

                    Struct.AssembleStructs(structs, fields);

                    file.Header = header;
                    file.Structs = structs;
                    file.Fields = fields;
                    file.Labels = labels;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading GFF file: {ex.Message}");
            }
            return file;
        }
    }

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

        public static Header Read(BinaryReader reader)
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

            Console.WriteLine($"File Type: {header.GFFType}, Version: {header.Version}");
            return header;
        }
    }

    public class Struct
    {
        public int Id { get; set; }
        public UInt32 DataOrDataOffset { get; set; }
        public UInt32 FieldCount { get; set; }
        public List<Field> Fields { get; set; }

        public static List<Struct> Read(BinaryReader reader, Header header)
        {
            List<Struct> structs = new List<Struct>();
            reader.BaseStream.Seek(header.StructOffset, SeekOrigin.Begin);

            while (reader.BaseStream.Position < header.FieldOffset)
            {
                int structId = (int)reader.ReadUInt32();
                uint dataOffset = reader.ReadUInt32();
                uint fieldCount = reader.ReadUInt32();

                structs.Add(new Struct
                {
                    Id = structId,
                    DataOrDataOffset = dataOffset,
                    FieldCount = fieldCount,
                    Fields = new List<Field>((int)fieldCount)
                });

                Console.WriteLine($"Struct.Type: {structId}, Struct.DataOrDataOffset: {dataOffset}, Struct.FieldCount: {fieldCount}");
            }

            return structs;
        }

        public static void AssembleStructs(List<Struct> structs, List<Field> fields)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                // Implementation of AssembleStructs logic here...
            }
        }
    }

    public class Field
    {
        public UInt32 Type { get; set; }
        public UInt32 LabelIndex { get; set; }
        public string Name { get; set; }
        public UInt32 DataOrDataOffset { get; set; }
        public object Value { get; set; }
        public int StructIndex { get; set; }

        public static List<Field> Read(BinaryReader reader, Header header, List<string> labels, List<Struct> structs)
        {
            List<Field> fields = new List<Field>();
            reader.BaseStream.Seek(header.FieldOffset, SeekOrigin.Begin);

            while (reader.BaseStream.Position < header.LabelOffset)
            {
                // Read all values first, then initialize field
                UInt32 type = reader.ReadUInt32();
                UInt32 labelIndex = reader.ReadUInt32();
                UInt32 dataOrDataOffset = reader.ReadUInt32();
                int structIndex = 1;

                Field field = new Field
                {
                    Type = type,
                    LabelIndex = labelIndex,
                    DataOrDataOffset = dataOrDataOffset,
                    Name = labels[(int)labelIndex]  // Use labelIndex here since it's already read
                };

                Console.Write($"Field.Type: {field.Type}, LabelIndex: {field.LabelIndex}, Label: {field.Name}, Value: ");

                // Store the reader position
                long pos = reader.BaseStream.Position;

                if (field.Type < 14)
                {
                    field.Value = InterpretField(reader, field.Type, field.DataOrDataOffset, header.FieldDataOffset);
                    Console.WriteLine(field.Value);
                    reader.BaseStream.Position = pos; // Restore position
                }
                else if (field.Type == (uint)FieldType.Struct)
                {
                    break; // Assuming we stop at the first struct type as in the original code
                }
                else if (field.Type == (uint)FieldType.List)
                {
                    field.Value = ReadList(reader, header, field.DataOrDataOffset, structs).Struct.Count;
                    // Console.WriteLine(field.Value);
                    reader.BaseStream.Position = pos; // Restore position
                }
                else
                {
                    throw new Exception($"Type not defined for type ID: {field.Type}");
                }

                fields.Add(field);
            }
            return fields;
        }

        private static object InterpretField(BinaryReader reader, uint typeId, uint dataOrOffset, uint fieldDataOffset)
        {
            switch ((FieldType)typeId)
            {
                case FieldType.Byte:
                    return (byte)dataOrOffset;
                case FieldType.Char:
                    return (char)dataOrOffset;
                case FieldType.Word:
                case FieldType.Short:
                case FieldType.Dword:
                case FieldType.Int:
                    return (int)dataOrOffset;
                case FieldType.Dword64:
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    return reader.ReadUInt64();
                case FieldType.Int64:
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    return reader.ReadInt64();
                case FieldType.Float:
                    return BitConverter.ToSingle(BitConverter.GetBytes(dataOrOffset), 0);
                case FieldType.Double:
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    return reader.ReadDouble();
                case FieldType.CExoString:
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    return ReadExoString(reader);
                case FieldType.ResRef:
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    byte length = reader.ReadByte();
                    return ReadResRefString(reader, length);
                case FieldType.CExoLocString:
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    uint totalLength = reader.ReadUInt32(); // Total length, might not be used
                    uint strRef = reader.ReadUInt32(); // TLK index
                    uint count = reader.ReadUInt32(); // Number of strings
                    return ReadExoLocString(reader, count);
                case FieldType.Void:
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    uint voidSize = reader.ReadUInt32();
                    return reader.ReadBytes((int)voidSize);
                case FieldType.Struct:
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    return reader.ReadInt64(); // Assuming struct ID is stored as Int64
                case FieldType.List:
                    return "List"; // Placeholder for list processing
                default:
                    return $"Unsupported TypeId ({typeId})!";
            }
        }

        private static GFFList ReadList(BinaryReader reader, Header header, uint offset, List<Struct> structs)
        {
            reader.BaseStream.Seek(header.ListIndicesOffset + offset, SeekOrigin.Begin);
            uint numStructsInList = reader.ReadUInt32();
            Console.WriteLine($"NumStructs in List: {numStructsInList}");

            GFFList gffList = new GFFList();
            for (int i = 0; i < numStructsInList; i++)
            {
                int structIndex = (int)reader.ReadUInt32();
                gffList.Struct.Add(structs[structIndex]);
            }

            return gffList;
        }

        public class GFFList
        {
            public List<Struct> Struct { get; set; } = new List<Struct>();
        }

        private static string ReadExoString(BinaryReader reader)
        {
            uint length = reader.ReadUInt32();
            return new string(reader.ReadChars((int)length));
        }

        private static string ReadResRefString(BinaryReader reader, byte length)
        {
            return new string(reader.ReadChars(length));
        }

        private static string ReadExoLocString(BinaryReader reader, uint numSubStrings)
        {
            uint strId = reader.ReadUInt32();
            uint length = reader.ReadUInt32();
            return new string(reader.ReadChars((int)length));
        }
    }

    public enum FieldType
    {
        Byte,
        Char,
        Word,
        Short,
        Dword,
        Int,
        Dword64,
        Int64,
        Float,
        Double,
        CExoString,
        ResRef,
        CExoLocString,
        Void,
        Struct,
        List
    }

    public class GFFList
    {
        public List<Struct> Struct { get; set; } = new List<Struct>();
    }

    public class Label
    {
        public static List<string> Read(BinaryReader reader, Header header)
        {
            List<string> labels = new List<string>();
            reader.BaseStream.Seek(header.LabelOffset, SeekOrigin.Begin);

            while (reader.BaseStream.Position < header.FieldDataOffset)
            {
                string label = ReadNullTerminatedString(reader);
                if (!string.IsNullOrEmpty(label))
                {
                    labels.Add(label);
                }
            }
            return labels;
        }

        private static string ReadNullTerminatedString(BinaryReader reader)
        {
            StringBuilder stringBuilder = new StringBuilder();
            char currentChar;
            while (stringBuilder.Length < 16)
            {
                if ((currentChar = reader.ReadChar()) != '\0')
                    stringBuilder.Append(currentChar);
                else
                    break;
            }
            return stringBuilder.ToString();
        }
    }

    public class GFFFile
    {
        public Header Header { get; set; }
        public List<Struct> Structs { get; set; }
        public List<Field> Fields { get; set; }
        public List<string> Labels { get; set; }
    }
}