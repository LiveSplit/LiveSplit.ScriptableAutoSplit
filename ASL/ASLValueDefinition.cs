using System;

namespace LiveSplit.ASL
{
    public class ASLValueDefinition
    {
        public String Type { get; set; }
        public String Identifier { get; set; }
        public DeepPointer Pointer { get; set; }
    }
}
