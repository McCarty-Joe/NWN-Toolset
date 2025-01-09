using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWN_Toolset
{
    public class GFFFile
    {
        public Header Header { get; set; }
        public List<Struct> Structs { get; set; }
        public List<Field> Fields { get; set; }
        public List<string> Labels { get; set; }
    }
}
