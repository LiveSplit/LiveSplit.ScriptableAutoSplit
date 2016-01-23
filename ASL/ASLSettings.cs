using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.ASL
{
    public class ASLSettings
    {
        public Dictionary<string, ASLSetting> Settings { get; set; }

        public ASLSettings()
        {
            Settings = new Dictionary<string, ASLSetting>();
        }

        public void AddSetting(string name, bool defaultValue, string description)
        {
            Settings.Add(name, new ASLSetting(name, defaultValue, description));
        }

        public bool GetSettingValue(string name)
        {
            return Settings[name].Enabled;
        }


    }
}
