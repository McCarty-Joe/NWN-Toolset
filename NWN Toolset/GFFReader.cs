using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWN_Toolset
{
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
}
