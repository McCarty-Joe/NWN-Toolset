using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWN_Toolset
{
    public class Struct
    {
        public int Id { get; set; }
        public UInt32 DataOrDataOffset { get; set; }
        public UInt32 FieldCount { get; set; }
        public List<Field> Fields { get; set; }

        public static List<Struct> Read(BinaryReader reader, Header header, bool verbose = true)
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

                if(verbose) Console.WriteLine($"Struct.Type: {structId}, Struct.DataOrDataOffset: {dataOffset}, Struct.FieldCount: {fieldCount}");
            }

            return structs;
        }

        public static void AssembleStructs(List<Struct> structs, List<Field> fields)
        {
            int n = 0;
            for (int i = 0; i < fields.Count; i++)
            {

                if (fields[i].Type == (int)FieldType.List)
                {
                    int nStructsInList = (int)fields[i].Value;
                    i++; // Move one field to get the first item after the list.
                    for (int j = 0; j < nStructsInList; j++)
                    {
                        
                        n++; // Next use next nested struct
                        for (int k = 0; k < structs[n].FieldCount; k++)
                        {
                            structs[n].Fields.Add(fields[i]);
                            i++;
                        }
                    }

                }
                else
                {
                    structs[0].Fields.Add(fields[i]);
                }
            }
        }
    }

}
