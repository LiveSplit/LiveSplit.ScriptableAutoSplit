using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.ASL
{
    public class ASLSettings
    {
        // Dict for easy access per key
        public Dictionary<string, ASLSetting> Settings { get; set; }
        // List for preserved insertion order (Dict provides that as well, but not guaranteed)
        public List<ASLSetting> OrderedSettings { get; }

        public Dictionary<string, ASLSetting> MethodSettings { get; }
        

        public ASLSettings()
        {
            Settings = new Dictionary<string, ASLSetting>();
            OrderedSettings = new List<ASLSetting>();
            MethodSettings = new Dictionary<string, ASLSetting>();
        }

        public void AddSetting(string name, bool defaultValue, string description)
        {
            ASLSetting setting = new ASLSetting(name, defaultValue, description);
            Settings.Add(name, setting);
            OrderedSettings.Add(setting);
        }

        public bool GetSettingValue(string name)
        {
            return Settings[name].Value;
        }

        public void AddMethodSetting(string name)
        {
            MethodSettings.Add(name, new ASLSetting(name, true, ""));
        }

        public bool MethodEnabled(string name)
        {
            if (MethodSettings.ContainsKey(name))
            {
                return MethodSettings[name].Value;
            }
            return false;
        }

        public bool MethodPresent(string name)
        {
            return MethodSettings.ContainsKey(name);
        }


    }
}
