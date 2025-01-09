using NWN_Toolset;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

namespace NWN_Toolset
{
    public class GFFWriter
    {
        public static void WriteGFFFile(GFFFile file, string outputFilePath)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(outputFilePath, FileMode.Create)))
            {
                // Write Header
                WriteHeader(writer, file.Header);

                // Write Struct Array
                long structArrayOffset = writer.BaseStream.Position;
                WriteStructs(writer, file.Structs);

                // Write Field Array
                long fieldArrayOffset = writer.BaseStream.Position;
                WriteFields(writer, file.Fields);

                // Write Label Array
                long labelArrayOffset = writer.BaseStream.Position;
                WriteLabels(writer, file.Labels);

                // Write Field Data Block (for this example, we'll just write the size, as actual data writing would be complex)
                long fieldDataOffset = writer.BaseStream.Position;
                WriteFieldData(writer, file.Fields);

                // Write Field Indices Array (simplified here)
                long fieldIndicesOffset = writer.BaseStream.Position;
                WriteFieldIndices(writer, file.Fields);

                // Write List Indices Array (simplified here)
                long listIndicesOffset = writer.BaseStream.Position;
                WriteListIndices(writer, file.Structs);

                // Update header with correct offsets
                UpdateHeader(writer, file.Header, structArrayOffset, fieldArrayOffset, labelArrayOffset,
                             fieldDataOffset, fieldIndicesOffset, listIndicesOffset);
            }
        }

        private static void WriteHeader(BinaryWriter writer, Header header)
        {
            writer.BaseStream.Seek(0, SeekOrigin.Begin); // Go back to start to overwrite header with updated offsets
            writer.Write(Encoding.UTF8.GetBytes(header.GFFType.PadRight(4, '\0')));
            writer.Write(Encoding.UTF8.GetBytes(header.Version.PadRight(4, '\0')));
            writer.Write((UInt32)0); // Placeholder for StructOffset, will be updated later
            writer.Write(header.StructCount);
            writer.Write((UInt32)0); // Placeholder for FieldOffset, will be updated later
            writer.Write(header.FieldCount);
            writer.Write((UInt32)0); // Placeholder for LabelOffset, will be updated later
            writer.Write(header.LabelCount);
            writer.Write((UInt32)0); // Placeholder for FieldDataOffset, will be updated later
            writer.Write(header.FieldDataCount);
            writer.Write((UInt32)0); // Placeholder for FieldIndicesOffset, will be updated later
            writer.Write(header.FieldIndicesCount);
            writer.Write((UInt32)0); // Placeholder for ListIndicesOffset, will be updated later
            writer.Write(header.ListIndicesCount);
        }

        private static void UpdateHeader(BinaryWriter writer, Header header, long structOffset, long fieldOffset,
                                         long labelOffset, long fieldDataOffset, long fieldIndicesOffset, long listIndicesOffset)
        {
            writer.BaseStream.Seek(8, SeekOrigin.Begin); // Move past FileType and Version
            writer.Write((UInt32)structOffset);
            writer.Write(header.StructCount);
            writer.Write((UInt32)fieldOffset);
            writer.Write(header.FieldCount);
            writer.Write((UInt32)labelOffset);
            writer.Write(header.LabelCount);
            writer.Write((UInt32)fieldDataOffset);
            writer.Write(header.FieldDataCount);
            writer.Write((UInt32)fieldIndicesOffset);
            writer.Write(header.FieldIndicesCount);
            writer.Write((UInt32)listIndicesOffset);
            writer.Write(header.ListIndicesCount);
        }

        private static void WriteStructs(BinaryWriter writer, List<Struct> structs)
        {
            foreach (var strct in structs)
            {
                writer.Write((UInt32)strct.Id);
                writer.Write(strct.DataOrDataOffset);
                writer.Write(strct.FieldCount);
            }
        }

        private static void WriteFields(BinaryWriter writer, List<Field> fields)
        {
            foreach (var field in fields)
            {
                writer.Write(field.Type);
                writer.Write(field.LabelIndex);
                writer.Write(field.DataOrDataOffset);
            }
        }

        private static void WriteLabels(BinaryWriter writer, List<string> labels)
        {
            foreach (var label in labels)
            {
                byte[] labelBytes = Encoding.UTF8.GetBytes(label);
                writer.Write(labelBytes);
                writer.Write(new byte[16 - labelBytes.Length]); // Padding to 16 bytes
            }
        }

        private static void WriteFieldData(BinaryWriter writer, List<Field> fields)
        {
            // This is a placeholder. Actual implementation would depend on each field's type
            writer.Write((UInt32)fields.Count); // Just writing count as a placeholder for data size
        }

        private static void WriteFieldIndices(BinaryWriter writer, List<Field> fields)
        {
            // Placeholder; actual indices would be based on how fields connect to data or other structures
            for (int i = 0; i < fields.Count; i++)
            {
                writer.Write((UInt32)i); // Example: just writing indices in order
            }
        }

        private static void WriteListIndices(BinaryWriter writer, List<Struct> structs)
        {
            // Placeholder; would need to handle lists properly if they exist in the file
            writer.Write((UInt32)0); // Writing 0 as there's no list in this example
        }
    }
}