#define GAME_TIME

using LiveSplit.ASL;
using LiveSplit.Model;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Options;

namespace LiveSplit.UI.Components
{
    class Component : LogicComponent
    {
        public ComponentSettings Settings { get; set; }

        public override string ComponentName => "Scriptable Auto Splitter";

        protected string OldScriptPath { get; set; }
        protected FileSystemWatcher FSWatcher { get; set; }
        protected bool DoReload { get; set; }
        protected Timer UpdateTimer { get; set; }

        public bool Refresh { get; set; }

        public ASLScript Script { get; set; }

        public Component(LiveSplitState state, string scriptPath)
            : this(state)
        {
            Settings = new ComponentSettings()
            {
                ScriptPath = scriptPath
            };
        }

        public Component(LiveSplitState state)
        {
            Settings = new ComponentSettings();
            FSWatcher = new FileSystemWatcher();
            FSWatcher.Changed += (sender, args) => DoReload = true;
            UpdateTimer = new Timer() { Interval = 15 }; // run a little faster than 60hz
            UpdateTimer.Tick += (sender, args) => UpdateScript(state);
            UpdateTimer.Enabled = true;
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {

        }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public override Control GetSettingsControl(LayoutMode mode)
        {
            return Settings;
        }

        public override void SetSettings(XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public override void Dispose()
        {
            if (FSWatcher != null)
                FSWatcher.Dispose();
            if (UpdateTimer != null)
                UpdateTimer.Dispose();
            if (Script != null)
                Script.RefreshRateChanged -= Script_RefreshRateChanged;
        }

        protected void UpdateScript(LiveSplitState state)
        {
            // this is ugly, fix eventually!
            if (Settings.ScriptPath != OldScriptPath && !string.IsNullOrEmpty(Settings.ScriptPath) || DoReload)
            {
                try
                {
                    DoReload = false;
                    OldScriptPath = Settings.ScriptPath;
                    FSWatcher.Path = Path.GetDirectoryName(Settings.ScriptPath);
                    FSWatcher.Filter = Path.GetFileName(Settings.ScriptPath);
                    FSWatcher.EnableRaisingEvents = true;
                    if (Script != null)
                        Script.RefreshRateChanged -= Script_RefreshRateChanged;
                    Script = ASLParser.Parse(File.ReadAllText(Settings.ScriptPath));
                    Script.RefreshRateChanged += Script_RefreshRateChanged;
                    Script_RefreshRateChanged(this, Script.RefreshRate);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }

            if (Script != null)
            {
                try
                {
                    Script.RunUpdate(state);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }

        private void Script_RefreshRateChanged(object sender, double e)
        {
            UpdateTimer.Interval = (int)Math.Round(1000 / e);
        }
    }
}
