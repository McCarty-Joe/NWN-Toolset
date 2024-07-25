using System;

namespace NWN_Toolset
{
    using System;
    using System.Collections.Generic;
    using System.Formats.Asn1;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Metadata.Ecma335;
    using System.Reflection.PortableExecutable;
    using System.Text;


    class Program
    {
        struct Header
        {
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
            internal UInt32 Type { get; set; }
            internal UInt32 DataOrDataOffset { get; set; }
            internal UInt32 FieldCount { get; set; }
        }

        struct Field
        {
            internal UInt32 Type { get; set; }
            internal UInt32 LabelIndex { get; set; }
            internal UInt32 DataOrDataOffset { get; set; }
        }

        static void Main()
        {
            //string filePath = "C:\\Users\\joemc\\Documents\\Neverwinter Nights\\modules\\temp0\\warham_crusher20.uti";
            //string filePath = "C:\\Users\\joemc\\source\\repos\\NWN Toolset\\GFF\\warham_crusher20.uti";
            string filePath = "C:\\Users\\joemc\\source\\repos\\NWN Toolset\\GFF\\abyssianvampire.utc";
            ReadUtiFile(filePath);
            Console.WriteLine($"Done reading GFF file: {filePath}");
        }

        static void ReadUtiFile(string filePath)
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
                {
                    //** GFF File Type & Version **//
                    string gffType = Encoding.UTF8.GetString(reader.ReadBytes(4));
                    if (gffType.Trim() != "UTI" && gffType.Trim() != "UTC") 
                    {
                        Console.WriteLine($"Invalid GFF file: {filePath}");
                        return;
                    }

                    string version = Encoding.UTF8.GetString(reader.ReadBytes(4));
                    if (version.Trim() != "V3.2")
                    {
                        Console.WriteLine($"Invalid GFF file: {filePath}");
                        return;
                    }

                    Console.WriteLine($"File Type: {gffType}, Version: {version}");

                    //** Header **//
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
                    header.StructOffset =  structOffset;
                    header.StructCount = structCount;   
                    header.FieldOffset = fieldOffset;   
                    header.FieldOffset = fieldCount;
                    header.LabelOffset = labelOffset;
                    header.LabelCount = labelCount;
                    header.FieldDataOffset = fieldDataOffset;
                    header.FieldDataCount = fieldDataCount;
                    header.FieldIndicesOffset = fieldIndicesOffset;
                    header.FieldIndicesCount = fieldIndicesCount;
                    header.ListIndicesOffset = listIndicesOffset;
                    header.ListIndicesCount = listIndicesCount;

                    //** Structs **//
                    List<Struct> structs = new List<Struct>();
                    
                    // Move to the beginning of the struct section
                    reader.BaseStream.Seek(header.StructOffset, SeekOrigin.Begin);
                    
                    // Read struct
                    while(reader.BaseStream.Position < fieldOffset)
                    {
                        uint structType = reader.ReadUInt32();
                        uint structDataOffset = reader.ReadUInt32();
                        uint structFieldCount = reader.ReadUInt32();

                        Struct gffstruct = new Struct();
                        gffstruct.Type = structType;
                        gffstruct.DataOrDataOffset = structDataOffset;
                        gffstruct.FieldCount = structFieldCount;

                        structs.Add(gffstruct);

                        Console.WriteLine($"Struct.Type: {structType}, Struct.DataOrDataOffset: {structDataOffset}, Struct.FieldCount: {structFieldCount}");                                                    
                    }

                    /** LABELS **/
                    List<string> labels = new List<string>();
                    
                    // Move to the beginning of the labels section
                    reader.BaseStream.Seek(labelOffset, SeekOrigin.Begin);

                    // Read labels
                    int i = 0;
                    while (reader.BaseStream.Position < fieldDataOffset)
                    {
                        string label = ReadNullTerminatedString(reader);
                        if (!String.IsNullOrEmpty(label))
                        {
                            labels.Add(label);                            
                            Console.WriteLine($"Label {i}: {label}");
                            i++;
                        }
                            
                    }

                    //** FIELD **//
                    List<Field> fields = new List<Field>();
                    // Move to the beginning of the field section
                    reader.BaseStream.Seek(fieldOffset, SeekOrigin.Begin);
                    // Read Labels
                    while(reader.BaseStream.Position < labelOffset)
                    {
                        Field field = new Field();
                        uint fieldTypeId = reader.ReadUInt32();
                        uint fieldLabelIndex = reader.ReadUInt32();
                        uint fieldDataOrOffset = reader.ReadUInt32();

                        Console.Write($"Field.Type: {fieldTypeId}, Field.LabelIndex: {fieldLabelIndex}, ");

                        long pos = reader.BaseStream.Position; 
                        if (fieldTypeId < 14)
                        {
                            string fieldValue = InterpretField(reader, fieldTypeId, fieldLabelIndex, fieldDataOrOffset, fieldDataOffset);
                            Console.WriteLine(fieldValue);
                        }
                        else if (fieldTypeId == 14)
                        {
                            
                        }
                        else if(fieldTypeId == 15)
                        {
                            
                            reader.BaseStream.Seek(listIndicesOffset + fieldDataOrOffset, SeekOrigin.Begin);
                            uint numStructsInList = reader.ReadUInt32();
                            Console.Write($"NumStructs in List: {numStructsInList}");

                            for(i = 0; i < numStructsInList; i++)
                            {

                            }
                                                        
                            //uint arr1 = reader.ReadUInt32(); // struct 1
                            //uint arr2 = reader.ReadUInt32(); // struct 2
                            //uint arr3 = reader.ReadUInt32(); // struct 3
                            //uint arr4 = reader.ReadUInt32(); // struct 4                            
                        }
                        else
                        {
                            throw new Exception("Type not defined");
                        }
                        
                        reader.BaseStream.Position = pos; // Restore position                      
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading GFF file: {ex.Message}");
            }
        }

        static string InterpretField(BinaryReader reader, uint typeId, uint fieldIndex, uint dataOrOffset, uint fieldDataOffset)
        {
            string fieldValue = string.Empty;
            switch (typeId)
            {
                case 0: // Byte
                    // Console.WriteLine($"Value: {dataOrOffset}");
                    fieldValue = dataOrOffset.ToString();
                    break;
                case 1: // Char
                    Console.WriteLine($"Value: {dataOrOffset}");
                    break;
                case 2: // WORD
                case 3: // SHORT
                case 4: // DWORD
                case 5: // INT
                    // Console.WriteLine($"Value: {dataOrOffset}");
                    fieldValue = dataOrOffset.ToString();
                    break;
                case 6: // DWORD64
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    // Console.WriteLine(reader.ReadUInt64());
                    fieldValue = reader.ReadUInt64().ToString();
                    break;
                case 7: // INT64
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    // Console.WriteLine($"Value: {reader.ReadInt64()}");
                    fieldValue = reader.ReadInt64().ToString();
                    break;
                case 8: // FLOAT
                    // Console.WriteLine($"Value: {dataOrOffset}");
                    fieldValue = dataOrOffset.ToString();
                    break;
                case 9: // DOUBLE
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    // Console.WriteLine($"Value: {reader.ReadDouble()}");
                    fieldValue = reader.ReadDouble().ToString();
                    break;
                case 10: // CExoString
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    // Console.WriteLine($"Value: {ReadExoString(reader)}");
                    fieldValue = ReadExoString(reader);
                    break;
                case 11: // CResRef
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    byte cResRefLength = reader.ReadByte(); // 16 max
                    // Console.WriteLine($"Value: {ReadResRefString(reader, cResRefLength)}");
                    fieldValue = ReadResRefString(reader, cResRefLength);
                    break;
                case 12: // CExoLocString
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    uint cExoLocStrLengthTotal = reader.ReadUInt32(); // Length of string should be 16
                    uint cExoLocStrRef = reader.ReadUInt32(); // Index in TLK file
                    uint cExoLocStrCount = reader.ReadUInt32();
                    // Console.WriteLine($"Value: {ReadExoLocString(reader, cExoLocStrCount)}");
                    if(cExoLocStrCount > 0) fieldValue = ReadExoLocString(reader, cExoLocStrCount);
                    break;
                case 13: // VOID
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Begin);
                    UInt32 voidSize = reader.ReadUInt32();
                    // Console.WriteLine($"Value: {reader.Read()}");
                    fieldValue = reader.ReadBytes((int)voidSize).ToString();
                    break;
                case 14: // STRUCT
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    // Console.WriteLine($"Value: {reader.ReadInt64()}");
                    fieldValue = reader.ReadInt64().ToString();
                    break;
                case 15: // LIST
                    reader.BaseStream.Position = fieldDataOffset;
                    reader.BaseStream.Seek(dataOrOffset, SeekOrigin.Current);
                    // Console.WriteLine($"Value: {reader.ReadInt32()}");
                    fieldValue = reader.ReadUInt32().ToString();
                    break;
                default:
                    // Console.WriteLine($"Unsupported Type! {typeId}");
                    fieldValue = $"Unsupported TypeId ({typeId})!";
                    break;

            }

            return fieldValue;
         }

        static void InterpretStruct(BinaryReader reader)
        {
            Console.WriteLine("interpret struct");
        }

        static void InterpretList(BinaryReader reader, uint size)
        {
            
        }

        static string ReadNullTerminatedString(BinaryReader reader)
        {
            StringBuilder stringBuilder = new StringBuilder();
            char currentChar;
            while ((currentChar = reader.ReadChar()) != '\0')
            {
                stringBuilder.Append(currentChar);
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

// string filePath = "C:\\Users\\joemc\\Documents\\Neverwinter Nights\\modules\\temp0\\warham_crusher20.uti";



