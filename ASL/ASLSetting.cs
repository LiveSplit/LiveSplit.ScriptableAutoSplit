namespace LiveSplit.ASL
{

    // Created from the ASL script and shared with the GUI to synchronize setting state.
    public class ASLSetting
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public bool Value { get; set; }
        public string Parent { get; }

        public ASLSetting(string id, bool defaultValue, string label, string parent)
        {
            Id = id;
            Value = defaultValue;
            Label = label;
            Parent = parent;
        }

        public override string ToString()
        {
            return Label;
        }
    }
}
