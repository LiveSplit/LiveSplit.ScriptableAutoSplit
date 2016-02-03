using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (description == null)
            {
                description = name;
            }
            ASLSetting setting = new ASLSetting(name, defaultValue, description);
            Settings.Add(name, setting);
            OrderedSettings.Add(setting);
        }

        public bool GetSettingValue(string name)
        {
            // Don't cause error if setting doesn't exist, but still inform script
            // author since that usually shouldn't happen.
            if (Settings.ContainsKey(name))
            {
                return Settings[name].Value;
            }
            Trace.WriteLine("[ASL] Custom Setting Key doesn't exist: "+name);
            return false;
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
