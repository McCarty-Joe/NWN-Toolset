using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWN_Toolset
{
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
}
