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

        public Dictionary<string, ASLSetting> BasicSettings { get; }

        public ASLSettingsBuilder Builder;
        public ASLSettingsReader Reader;
        
        public ASLSettings()
        {
            Settings = new Dictionary<string, ASLSetting>();
            OrderedSettings = new List<ASLSetting>();
            BasicSettings = new Dictionary<string, ASLSetting>();
            Builder = new ASLSettingsBuilder(this);
            Reader = new ASLSettingsReader(this);
        }

        public void AddSetting(string name, bool defaultValue, string description, string parent)
        {
            if (description == null)
            {
                description = name;
            }
            if (parent != null && !Settings.ContainsKey(parent))
            {
                throw new ArgumentException("Parent for setting '"+name+"' is not a setting: " + parent);
            }
            ASLSetting setting = new ASLSetting(name, defaultValue, description, parent);
            Settings.Add(name, setting);
            OrderedSettings.Add(setting);
        }

        public bool GetSettingValue(string name)
        {
            // Don't cause error if setting doesn't exist, but still inform script
            // author since that usually shouldn't happen.
            if (Settings.ContainsKey(name))
            {
                return getSettingValueRecursive(Settings[name]);
            }
            Trace.WriteLine("[ASL] Custom Setting Key doesn't exist: "+name);
            return false;
        }

        /// <summary>
        /// Returns true only if this setting and all it's parent settings are true.
        /// </summary>
        private bool getSettingValueRecursive(ASLSetting setting)
        {
            if (!setting.Value)
            {
                return false;
            }
            if (setting.Parent == null)
            {
                return setting.Value;
            }
            return getSettingValueRecursive(Settings[setting.Parent]);
        }

        public void AddBasicSetting(string name)
        {
            BasicSettings.Add(name, new ASLSetting(name, true, "", null));
        }

        public bool GetBasicSettingValue(string name)
        {
            if (BasicSettings.ContainsKey(name))
            {
                return BasicSettings[name].Value;
            }
            return false;
        }

        public bool IsBasicSettingPresent(string name)
        {
            return BasicSettings.ContainsKey(name);
        }
    }

    /// <summary>
    /// Interface for adding settings via the ASL Script.
    /// </summary>
    public class ASLSettingsBuilder
    {
        public string CurrentDefaultParent { get; set; }
        ASLSettings s;

        public ASLSettingsBuilder(ASLSettings s)
        {
            this.s = s;
        }

        public void Add(string id, bool defaultValue = true, string description = null, string parent = null)
        {
            if (parent == null)
            {
                parent = CurrentDefaultParent;
            }
            s.AddSetting(id, defaultValue, description, parent);
        }
    }

    /// <summary>
    /// Interface for reading settings via the ASL Script.
    /// </summary>
    public class ASLSettingsReader
    {
        ASLSettings s;

        public ASLSettingsReader(ASLSettings s)
        {
            this.s = s;
        }

        public dynamic this[string id]
        {
            get
            {
                return s.GetSettingValue(id);
            }
        }
    }

}
