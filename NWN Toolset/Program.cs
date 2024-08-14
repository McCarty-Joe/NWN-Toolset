using System;

namespace NWN_Toolset
{
    using Microsoft.VisualBasic.FileIO;
    using System;
    using System.Collections.Generic;
    using System.Formats.Asn1;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Reflection.Metadata.Ecma335;
    using System.Reflection.PortableExecutable;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Text;


    class GFF
    {
        enum FieldType
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
            
        struct Header
        {
            internal string GFFType { get; set; }
            internal string Version { get; set; }
            internal UInt32 StructOffset { get; set; }
            internal UInt32 StructCount { get; set; }
            internal UInt32 FieldOffset { get; set; }
            internal UInt32 FieldCount { get; set; }
            internal UInt32 LabelOffset { get; set; }
            internal UInt32 LabelCount { get; set; }
            internal UInt32 FieldDataOffset { get; set; }
            internal UInt32 FieldDataCount { get; set; }
            internal UInt32 FieldIndicesOffset { get; set; }
            internal UInt32 FieldIndicesCount { get; set; }
            internal UInt32 ListIndicesOffset { get; set; }
            internal UInt32 ListIndicesCount { get; set; }
        };

        struct Struct
        {
            internal int Id { get; set; }
            internal UInt32 DataOrDataOffset { get; set; }
            internal UInt32 FieldCount { get; set; }
            internal List<Field> Field { get; set; }
        }

        struct Field
        {
            internal UInt32 Type { get; set; }
            internal UInt32 LabelIndex { get; set; }
            internal string Name { get; set; }
            internal UInt32 DataOrDataOffset { get; set; }
            internal object Value { get; set; }
                        
        }

        struct GFFList()
        {
            internal List<Struct> Struct { get; set; } = new List<Struct>();
        }

        struct GFFFile
        {
            internal Header header;
            internal List<Struct> structs;
            internal List<Field> fields;
            internal List<string> labels;
        }

        static void Main(string[] args)
        {
           //string filePath = "C:\\Users\\joemc\\Documents\\Neverwinter Nights\\modules\\temp0\\warham_crusher20.uti";
            // string filePath = "C:\\Users\\joemc\\source\\repos\\NWN Toolset\\GFF\\warham_crusher20.uti";
            string filePath = "C:\\Users\\joemc\\source\\repos\\NWN Toolset\\GFF\\abyssianvampire.utc";
            // string filePath = "C:\\Users\\joemc\\source\\repos\\NWN Toolset\\GFF\\creaturepalcus.itp";
            GFFFile file = ReadGFFFile(filePath);
            Console.WriteLine($"Done reading GFF file: {filePath}");
        }

        static GFFFile ReadGFFFile(string filePath, bool verbose = false)
        {
            GFFFile file = new GFFFile();
            try
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
                {
                    long fileSize = reader.BaseStream.Length;
                    
                    //** Header **//
                    Header header = ReadHeaders(reader);

                    //** Structs **//
                    List<Struct> structs = ReadStructs(reader, header); 

                    /** Labels **/
                    List<string> labels = ReadLabels(reader, header);

                    //** Field **//
                    List<Field> fields = ReadFields(reader, header, labels, structs);

                    AssembleStructs(structs, fields);         
                    
                    file.header = header;
                    file.structs = structs;
                    file.fields = fields;
                    file.labels = labels;

                    Console.WriteLine("done reading file");
                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading GFF file: {ex.Message}");
            }

            return file;
        }

        static Header ReadHeaders(BinaryReader reader)
        {
            reader.BaseStream.Position = 0;

            string gffType = Encoding.UTF8.GetString(reader.ReadBytes(4));
            string version = Encoding.UTF8.GetString(reader.ReadBytes(4));

            //** GFF File Type & Version **//
            //string gffType = Encoding.UTF8.GetString(reader.ReadBytes(4));
            //if (gffType.Trim() != "UTI" && gffType.Trim() != "UTC")
            //{
            //    Console.WriteLine($"Invalid GFF file: {filePath}");
            //    return;
            //}

            //string version = Encoding.UTF8.GetString(reader.ReadBytes(4));
            //if (version.Trim() != "V3.2")
            //{
            //    Console.WriteLine($"Invalid GFF file: {filePath}");
            //    return;
            //}

            Console.WriteLine($"File Type: {gffType}, Version: {version}");
            UInt32 structOffset = reader.ReadUInt32();
            UInt32 structCount = reader.ReadUInt32();
            UInt32 fieldOffset = reader.ReadUInt32();
            UInt32 fieldCount = reader.ReadUInt32();
            UInt32 labelOffset = reader.ReadUInt32();
            UInt32 labelCount = reader.ReadUInt32();
            UInt32 fieldDataOffset = reader.ReadUInt32();
            UInt32 fieldDataCount = reader.ReadUInt32();
            UInt32 fieldIndicesOffset = reader.ReadUInt32();
            UInt32 fieldIndicesCount = reader.ReadUInt32();
            UInt32 listIndicesOffset = reader.ReadUInt32();
            UInt32 listIndicesCount = reader.ReadUInt32();

            Header header = new Header();
            header.GFFType = gffType;
            header.Version = version;
            header.StructOffset = structOffset;
            header.StructCount = structCount;
            header.FieldOffset = fieldOffset;
            header.FieldCount = fieldCount;
            header.LabelOffset = labelOffset;
            header.LabelCount = labelCount;
            header.FieldDataOffset = fieldDataOffset;
            header.FieldDataCount = fieldDataCount;
            header.FieldIndicesOffset = fieldIndicesOffset;
            header.FieldIndicesCount = fieldIndicesCount;
            header.ListIndicesOffset = listIndicesOffset;
            header.ListIndicesCount = listIndicesCount;

            return header;
        }

        static List<Struct> ReadStructs(BinaryReader reader, Header header)
        {
            reader.BaseStream.Seek(header.StructOffset, SeekOrigin.Begin);

            List<Struct> structs = new List<Struct>();
            while (reader.BaseStream.Position < header.FieldOffset)
            {
                int structId = (int)reader.ReadUInt32();

                if (structId == reader.BaseStream.Length)
                    structId = -1;

                uint structDataOffset = reader.ReadUInt32();
                uint structFieldCount = reader.ReadUInt32();

                Struct gffstruct = new Struct();
                gffstruct.Id = structId;
                gffstruct.DataOrDataOffset = structDataOffset;
                gffstruct.FieldCount = structFieldCount;
                gffstruct.Field = new List<Field>((int)structFieldCount);

                structs.Add(gffstruct);

                Console.WriteLine($"Struct.Type: {structId}, Struct.DataOrDataOffset: {structDataOffset}, Struct.FieldCount: {structFieldCount}");
            }

            return structs;
        }

        static List<string> ReadLabels(BinaryReader reader, Header header)
        {
            List<string> labels = new List<string>();
            reader.BaseStream.Seek(header.LabelOffset, SeekOrigin.Begin);

            // Read labels
            while (reader.BaseStream.Position < header.FieldDataOffset)
            {
                string label = ReadNullTerminatedString(reader);
                if (!string.IsNullOrEmpty(label))
                {
                    labels.Add(label);
                }
            }
            return labels; // Ordered by field index
        }

        static object InterpretField(BinaryReader reader, uint typeId, uint dataOrOffset, uint fieldDataOffset)
        {
            object fieldValue = 0;
            switch ((GFF.FieldType)typeId)
            {
                case FieldType.Byte: // Byte
                    fieldValue = (byte)dataOrOffset;
                    // Console.WriteLine($"Value: {dataOrOffset}");
                    break;
                case FieldType.Char: // Char
                    fieldValue = (char)dataOrOffset;
                    // Console.WriteLine($"Value: {dataOrOffset}");
                    break;
                case FieldType.Word: // WORD
                case FieldType.Short: // SHORT
                case FieldType.Dword: // DWORD
                case FieldType.Int: // INT
                    // Console.WriteLine($"Value: {dataOrOffset}");
                    fieldValue = (int)dataOrOffset;
                    break;
                case FieldType.Dword64: // DWORD64
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    // Console.WriteLine(reader.ReadUInt64());
                    fieldValue = reader.ReadUInt64();
                    break;
                case FieldType.Int64: // INT64
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    // Console.WriteLine($"Value: {reader.ReadInt64()}");
                    fieldValue = reader.ReadInt64();
                    break;
                case FieldType.Float: // FLOAT
                    // Console.WriteLine($"Value: {dataOrOffset}");
                    fieldValue = (float)dataOrOffset;
                    break;
                case FieldType.Double: // DOUBLE
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    // Console.WriteLine($"Value: {reader.ReadDouble()}");
                    fieldValue = reader.ReadDouble();
                    break;
                case FieldType.CExoString: // CExoString
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    // Console.WriteLine($"Value: {ReadExoString(reader)}");
                    fieldValue = ReadExoString(reader);
                    break;
                case FieldType.ResRef: // CResRef
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    byte cResRefLength = reader.ReadByte(); // 16 max
                    // Console.WriteLine($"Value: {ReadResRefString(reader, cResRefLength)}");
                    fieldValue = ReadResRefString(reader, cResRefLength);
                    break;
                case FieldType.CExoLocString: // CExoLocString
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    uint cExoLocStrLengthTotal = reader.ReadUInt32(); // Length of string should be 16
                    uint cExoLocStrRef = reader.ReadUInt32(); // Index in TLK file
                    uint cExoLocStrCount = reader.ReadUInt32();
                    // Console.WriteLine($"Value: {ReadExoLocString(reader, cExoLocStrCount)}");
                    if(cExoLocStrCount > 0) fieldValue = ReadExoLocString(reader, cExoLocStrCount);
                    break;
                case FieldType.Void: // VOID
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    UInt32 voidSize = reader.ReadUInt32();
                    // Console.WriteLine($"Value: {reader.Read()}");
                    fieldValue = reader.ReadBytes((int)voidSize);
                    break;
                case FieldType.Struct: // STRUCT
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    // Console.WriteLine($"Value: {reader.ReadInt64()}");
                    fieldValue = reader.ReadInt64();
                    break;
                case FieldType.List: // LIST
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    // Console.WriteLine($"Value: {reader.ReadInt32()}");
                    fieldValue = reader.ReadUInt32();
                    break;
                default:
                    // Console.WriteLine($"Unsupported Type! {typeId}");
                    fieldValue = $"Unsupported TypeId ({typeId})!";
                    break;

            }

            return fieldValue;
         }

        static List<Field> ReadFields(BinaryReader reader, Header header, List<string> labels, List<Struct> structs)
        {

            List<Field> fields = new List<Field>();
            // Move to the beginning of the field section
            reader.BaseStream.Seek(header.FieldOffset, SeekOrigin.Begin);
            // Read Labels
            while (reader.BaseStream.Position < header.LabelOffset)
            {
                Field field = new Field();

                uint fieldTypeId = reader.ReadUInt32();
                uint fieldLabelIndex = reader.ReadUInt32();
                uint fieldDataOrOffset = reader.ReadUInt32();
                string label = labels[(int)fieldLabelIndex];

                field.Type = fieldTypeId;
                field.LabelIndex = fieldLabelIndex;
                field.DataOrDataOffset = fieldDataOrOffset;
                field.Name = label;


                Console.Write($"Field.Type: {fieldTypeId}, LabelIndex: {fieldLabelIndex}, Label: {label}, Value: ");

                // Store the reader position
                long pos = reader.BaseStream.Position;

                if (fieldTypeId < 14)
                {
                    object fieldValue = InterpretField(reader, fieldTypeId, fieldDataOrOffset, header.FieldDataOffset);
                    field.Value = fieldValue;
                    Console.WriteLine(fieldValue);
                    reader.BaseStream.Position = pos; // Restore position
                }
                else if (fieldTypeId == (int)FieldType.Struct)
                {
                    break;
                }
                else if (fieldTypeId == (int)FieldType.List)
                {
                    // Set the position 
                    // reader.BaseStream.Seek(header.ListIndicesOffset + fieldDataOrOffset, SeekOrigin.Begin);
                    //uint numStructsInList = reader.ReadUInt32();
                    //field.Value = numStructsInList.ToString();
                    //Console.WriteLine($"NumStructs in List: {numStructsInList}");

                    object fieldValue = ReadList(reader, header, fieldDataOrOffset, structs);
                    field.Value = fieldValue;

                    // return the position
                    reader.BaseStream.Position = pos; // Restore position


                }
                else
                {
                    throw new Exception("Type not defined");
                }

                fields.Add(field);


            }
            return fields;
        }

        static GFFList ReadList(BinaryReader reader, Header header, uint offset, List<Struct> structs)
        {
            reader.BaseStream.Seek(header.ListIndicesOffset + offset, SeekOrigin.Begin);

            uint numStructsInList = reader.ReadUInt32();
            Console.WriteLine($"NumStructs in List: {numStructsInList}");

            GFFList gFFList = new GFFList();
            for (int i = 0; i < numStructsInList; i++)
            {
                int structIndex = (int)reader.ReadUInt32();
                gFFList.Struct.Add(structs[structIndex]);
            }

            return gFFList;
        }

        static List<Struct> AssembleStructs(List<Struct> structs, List<Field> fields)
        {
            for(int i = 0; i < fields.Count;i++) 
            {
                if (fields[i].Type == (uint)FieldType.List)
                {
                    // This is a list. First determine how many structs are in it.
                    GFFList tmpList = (GFFList)fields[i].Value;
                    int structCount = tmpList.Struct.Count;

                    // Now add the list to the top level struct
                    structs[0].Field.Add(fields[i]);
                    // Remember the placement of this field in the top level struct
                    int fieldIndex = i;
                    // Iterate to the next field which should be in a sub-struct
                    i++;

                    // Assign the fields to the sub-struct
                    foreach (Struct s in tmpList.Struct)
                    {
                        // Get the number of fields in this struct
                        int fieldCount = (int)s.FieldCount;
                        for(int j = 0;j< fieldCount; j++)
                        {
                            s.Field.Add(fields[i]);
                            if (j < fieldCount) i++; // Increment unless we're on the last one.
                        }
                    }
                    i--;

                    // Finally replace the original list with the tmp list which as the fields
                    // structs[0].Field[fieldIndex].Value = tmpList;
                }
                else 
                {
                    // Add to top level struct
                    structs[0].Field.Add(fields[i]);
                }

                
            }

            return structs;
        }
        
        static string ReadNullTerminatedString(BinaryReader reader)
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

        static string ReadResRefString(BinaryReader reader, uint length)
        {
            StringBuilder stringBuilder = new StringBuilder();
            char currentChar;
            for(int i = 0; i < length; i++)
            {
                currentChar = reader.ReadChar();
                stringBuilder.Append(currentChar);
            }
            return stringBuilder.ToString();
        }

        static string ReadExoString(BinaryReader reader)
        {
            uint length = reader.ReadUInt32();

            StringBuilder stringBuilder = new StringBuilder();
            char currentChar;
            for (int i = 0; i < length; i++)
            {
                currentChar = reader.ReadChar();
                stringBuilder.Append(currentChar);
            }
            return stringBuilder.ToString();
        }

        static string ReadExoLocString(BinaryReader reader, uint numSubStrings)
        {
            uint stringId = reader.ReadUInt32(); // 2 times the lang ID + Gender (0=f, 1=m)
            uint length = reader.ReadUInt32(); 
            
            StringBuilder stringBuilder = new StringBuilder();
            char currentChar;
            for (int i = 0; i < length; i++)
            {
                currentChar = reader.ReadChar();
                stringBuilder.Append(currentChar);
            }
            return stringBuilder.ToString();
        }
                
    }
}





