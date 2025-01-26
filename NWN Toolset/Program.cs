using System;
using System.IO;
using System.Linq;
using static System.Net.WebRequestMethods;

namespace NWN_Toolset
{
    class Program
    {
        static void Main(string[] args)
        {
            /* Single file test
           
            string filePath = "C:\\Users\\joemc\\Source\\repos\\NWN Toolset\\GFF\\js_bndofstruct40.uti";
            GFFFile file = GFFReader.ReadGFFFile(filePath);

            //GFFModifier.ModifyItemProperty(filePath, "PaletteID", 23, 221, [65]); // w/ base item restriction

            // ItemProperty seeInvisibility = ItemProperty.CreateSeeInvisibility();

            //GFFModifier.ModifyItemPropertiesList(filePath, ItemProperty.Property.TrueSeeing, seeInvisibility, 30684); // 30684

            // GFFModifier.ModifyItemProperty(filePath, "Cost", 37212, 30684);

            //  GFFFile file = GFFReader.ReadGFFFile(filePath);
            // Console.WriteLine($"Done reading GFF file: {filePath}");
            */

            string directoryPath = "C:\\Users\\joemc\\Documents\\Neverwinter Nights\\modules\\temp0";

              // Ensure the directory exists
              if (!Directory.Exists(directoryPath))
              {
                  Console.WriteLine($"Directory does not exist: {directoryPath}");
                  return;
              }

              // Get all .uti files in the directory
              string[] utiFiles = Directory.GetFiles(directoryPath, "*.uti");

              if (utiFiles.Length == 0)
              {
                  Console.WriteLine("No .uti files found in the directory.");
                  return;
              }

              // Process each .uti file
              foreach (string filePath in utiFiles)
              {
                  try
                  {
                    //Console.WriteLine($"Processing file: {filePath}");
                    if (args[0] == "r")
                    {
                        GFFFile file = GFFReader.ReadGFFFile(filePath, false);
                        
                        // TODO: Move property finder to a class
                        for (int i = 1; i < file.Structs.Count; i++)
                        {

                            if (file.Structs[i].Fields[0].Name == "PropertyName" && (int)file.Structs[i].Fields[0].Value == 20)
                            {
                                for (int j = 1; j < file.Structs[i].Fields.Count; j++)
                                {
                                    if (file.Structs[i].Fields[j].Name == "CostValue" && (int)file.Structs[i].Fields[j].Value > 2)
                                    {
                                        string itemName = (string)file.Structs[0].Fields.FirstOrDefault(f => f.Name == "LocalizedName")?.Value;
                                        Console.WriteLine(itemName);
                                    }
                                }

                            }
                        }
                    }
                    else if (args[0] == "m")
                    {
                        //GFFModifier.ModifyItemProperty(filePath, "PaletteID", -1, 221, new int[] { 65 }); // Keys to the key node
                        ItemProperty seeInvisibility = ItemProperty.CreateSeeInvisibility();
                        GFFModifier.ModifyItemPropertiesList(filePath, ItemProperty.Property.TrueSeeing, seeInvisibility, 32034, false); //true seeing to see inviz x3/day w/ cost 32034
                                                                                                                                         // Console.WriteLine($"Successfully processed: {filePath}");
                    }
                  }
                  catch (Exception ex)
                  {
                      Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                  }
              }

               Console.WriteLine("All files processed.");
              

        }
    }

}