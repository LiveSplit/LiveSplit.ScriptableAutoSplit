using LiveSplit.ComponentUtil;

namespace LiveSplit.ASL
{
    public class ASLValueDefinition
    {
        public string Type { get; set; }
        public string Identifier { get; set; }
        public DeepPointer Pointer { get; set; }
    }
}
