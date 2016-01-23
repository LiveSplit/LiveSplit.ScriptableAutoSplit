namespace LiveSplit.ASL
{
    public class ASLSetting
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public bool Enabled { get; set; }

        public ASLSetting(string name, bool defaultValue, string label)
        {
            Name = name;
            Enabled = defaultValue;
            Label = label;
        }

        public override string ToString()
        {
            return Label;
        }
    }
}
