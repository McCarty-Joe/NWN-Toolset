using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NWN_Toolset
{
    public class GFFModifier
    {
        public static void ModifyItemProperty(string filePath, string propertyName, object oldValue, object newValue)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                using (var reader = new BinaryReader(stream))
                using (var writer = new BinaryWriter(stream))
                {
                    Header header = Header.Read(reader);

                    // Read Structs to know where Fields start
                   //List<Struct> structs = Struct.Read(reader, header);

                    // Move to Fields
                    reader.BaseStream.Seek(header.FieldOffset, SeekOrigin.Begin);

                    long fieldStartPosition = header.FieldOffset;
                    long fieldPosition = fieldStartPosition;

                    for (int i = 0; i < header.FieldCount; i++)
                    {
                        UInt32 type = reader.ReadUInt32();
                        UInt32 labelIndex = reader.ReadUInt32();
                        UInt32 dataOrOffset = reader.ReadUInt32();

                        if (type < 14) // Simple types where value is directly in dataOrOffset
                        {
                            // Remember the current position before moving the reader
                            fieldPosition = reader.BaseStream.Position;

                            // Assuming labelIndex is the index into the Label array
                            List<string> labels = Label.Read(reader, header); // This would read labels at the correct position
                            string label = labels[(int)labelIndex];
                            string itemName = string.Empty;

                            if(label == "LocalizedName")
                            {
                                
                            }

                            if (label == propertyName && ((int)oldValue == -1 || dataOrOffset == (uint)Convert.ToInt32(oldValue)))
                            {
                                // Found our field, remember the position
                                reader.BaseStream.Seek(fieldPosition - 4, SeekOrigin.Begin); // Go back 4 bytes to the start of dataOrOffset
                                long modifyPosition = reader.BaseStream.Position;
                                writer.Write((UInt32)Convert.ToInt32(newValue)); // Overwrite with new value
                                Console.WriteLine($"Modified {itemName}'s '{propertyName}' from {oldValue} to {newValue} at position {modifyPosition}");
                                return;
                            }

                            reader.BaseStream.Position = fieldPosition;
                        }


                    }
                    Console.WriteLine($"No field with name '{propertyName}' and value {oldValue} found to modify.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error modifying GFF file: {ex.Message}");
            }
        }

        public static void ModifyItemProperty(string filePath, string propertyName, object oldValue, object newValue, int[] baseItemType = null)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                using (var reader = new BinaryReader(stream))
                using (var writer = new BinaryWriter(stream))
                {
                    Header header = Header.Read(reader);

                    // Read Structs to know where Fields start
                    List<Struct> structs = Struct.Read(reader, header);

                    // Move to Fields
                    reader.BaseStream.Seek(header.FieldOffset, SeekOrigin.Begin);

                    long fieldStartPosition = header.FieldOffset;
                    long fieldPosition = fieldStartPosition;

                    for (int i = 0; i < header.FieldCount; i++)
                    {
                        UInt32 type = reader.ReadUInt32();
                        UInt32 labelIndex = reader.ReadUInt32();
                        UInt32 dataOrOffset = reader.ReadUInt32();

                        
                        if (type < 14) // Simple types where value is directly in dataOrOffset
                        {
                            // Remember the current position before moving the reader
                            fieldPosition = reader.BaseStream.Position;

                            // Assuming labelIndex is the index into the Label array
                            List<string> labels = Label.Read(reader, header); // This would read labels at the correct position
                            string label = labels[(int)labelIndex];
                            string itemName = string.Empty;

                            if (label == "LocalizedName")
                            {
                                
                            }

                            // Only do if we have specified a base item type and matches
                            if (label == "BaseItem" && baseItemType != null)
                            {
                                bool isItemType = false;
                                foreach (int baseItem in baseItemType)
                                {
                                    // Is it the base item type we're looking for?
                                    if ((uint)Convert.ToInt32(baseItem) == dataOrOffset)
                                        isItemType = true;
                                }

                                if (!isItemType) return;
                            }

                            if (label == propertyName && ((int)oldValue == -1 || dataOrOffset == (uint)Convert.ToInt32(oldValue)))
                            {
                                // Found our field, remember the position
                                reader.BaseStream.Seek(fieldPosition - 4, SeekOrigin.Begin); // Go back 4 bytes to the start of dataOrOffset
                                long modifyPosition = reader.BaseStream.Position;
                                writer.Write((UInt32)Convert.ToInt32(newValue)); // Overwrite with new value
                                Console.WriteLine($"Modified '{propertyName}' from {oldValue} to {newValue} at position {modifyPosition}");
                                return;
                            }
                            
                            reader.BaseStream.Position = fieldPosition;
                        }

                        
                    }
                    Console.WriteLine($"No field with name '{propertyName}' and value {oldValue} found to modify.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error modifying GFF file: {ex.Message}");
            }
        }

        public static void ModifyItemPropertiesList(string filePath, ItemProperty.Property currentIP, ItemProperty newIP, int addCost = 0)
        {
            try
            {
                GFFFile file = GFFReader.ReadGFFFile(filePath);
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                using (var reader = new BinaryReader(stream))
                using (var writer = new BinaryWriter(stream))
                {
                    // Read headers to know where the sections start
                    // Header header = Header.Read(reader);
                    Header header = file.Header;

                    // Read Structs to know where Fields start
                    //List<Struct> structs = Struct.Read(reader, header);

                    // Create a label index
                    // List<string> labels = Label.Read(reader, header);
                    List<string> labels = file.Labels;

                    List<Field> fields = file.Fields;

                    reader.BaseStream.Seek(header.FieldOffset, SeekOrigin.Begin);

                    long fieldStartPosition = header.FieldOffset;
                    long fieldPosition = fieldStartPosition;
                    long? costPosition = null;
                    long? namePosition = null;

                    for (int i = 0; i < header.FieldCount; i++)
                    {
                        UInt32 type = reader.ReadUInt32();
                        UInt32 labelIndex = reader.ReadUInt32();
                        UInt32 dataOrOffset = reader.ReadUInt32();

                        if (type < 14) // Simple types where value is directly in dataOrOffset
                        {
                            // Remember the current position before moving the reader
                            fieldPosition = reader.BaseStream.Position;

                            string label = labels[(int)labelIndex];

                            
                            string itemName = string.Empty;

                            if (label == "LocalizedName")
                            {
                                itemName = (string)fields[i].Value; 
                            }

                            if (label == "AddCost" && addCost > 0)
                            {
                                // Found our field, remember the position
                                reader.BaseStream.Seek(fieldPosition - 4, SeekOrigin.Begin); // Go back 4 bytes to the start of dataOrOffset
                                costPosition = reader.BaseStream.Position;                                
                            }

                            if (label == "PropertyName" && dataOrOffset == (uint)currentIP)
                            {
                                // Modify the 7 values. Set up by going back to the property name value
                                reader.BaseStream.Seek(fieldPosition - 4, SeekOrigin.Begin); // Go back 4 bytes to the start of dataOrOffset
                                long modifyPosition = reader.BaseStream.Position;
                                foreach(var property in newIP)
                                {
                                    
                                    writer.Write((UInt32)Convert.ToInt32(property.Value));
                                    UInt32 whatisthis1 = reader.ReadUInt32();
                                    UInt32 whatisthis2 = reader.ReadUInt32();
                                }

                                if(addCost > 0 && costPosition != null)
                                {
                                    reader.BaseStream.Seek((long)costPosition, SeekOrigin.Begin);
                                    writer.Write((UInt32)Convert.ToInt32(addCost)); // Overwrite with new value                                    
                                }

                                Console.WriteLine($"Modified {itemName}");

                                return;
                            }

                            reader.BaseStream.Position = fieldPosition;
                        }


                    }
                    Console.WriteLine($"No field Properties list with the property name {currentIP} found to modify.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error modifying GFF file: {ex.Message}");
            }
        }
    }
}
