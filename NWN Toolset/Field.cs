using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWN_Toolset
{
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
}
