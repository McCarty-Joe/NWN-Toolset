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
            /*
             //string filePath = "C:\\Users\\joemc\\Documents\\Neverwinter Nights\\modules\\temp0\\item028.uti";
            string filePath = "C:\\Users\\joemc\\Source\\repos\\NWN Toolset\\GFF\\HelmetofTesting.uti";
             GFFFile file2 = GFFReader.ReadGFFFile(filePath);
            
             // GFFModifier.ModifyItemProperty(filePath, "PropertyName", 71, 35);

            //GFFModifier.ModifyItemProperty(filePath, "PaletteID", 23, 221, [65]); // w/ base item restriction
            
            ItemProperty seeInvisibility = ItemProperty.CreateSeeInvisibility();

            //GFFModifier.ModifyItemPropertiesList(filePath, ItemProperty.Property.TrueSeeing, seeInvisibility, 30684); // 30684


            
            // GFFModifier.ModifyItemProperty(filePath, "Cost", 37212, 30684);
           //  GFFFile file = GFFReader.ReadGFFFile(filePath);
            Console.WriteLine($"Done reading GFF file: {filePath}");
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
                       GFFReader.ReadGFFFile(filePath);
                   else if (args[0] == "m")
                   {
                       //GFFModifier.ModifyItemProperty(filePath, "PaletteID", -1, 221, new int[] { 65 }); // Keys to the key node
                       ItemProperty seeInvisibility = ItemProperty.CreateSeeInvisibility();
                       GFFModifier.ModifyItemPropertiesList(filePath, ItemProperty.Property.TrueSeeing, seeInvisibility, 32034); //true seeing to see inviz x3/day w/ cost 32034
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