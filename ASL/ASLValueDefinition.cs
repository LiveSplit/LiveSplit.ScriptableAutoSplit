using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.ASL
{
    public class ASLValueDefinition
    {
        public String Type { get; set; }
        public String Identifier { get; set; }
        public DeepPointer Pointer { get; set; }
    }
}
