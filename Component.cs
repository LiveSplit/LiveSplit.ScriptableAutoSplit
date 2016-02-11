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

        protected LiveSplitState State { get; }
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
            State = state;
            Settings = new ComponentSettings();
            FSWatcher = new FileSystemWatcher();
            FSWatcher.Changed += (sender, args) => DoReload = true;
            UpdateTimer = new Timer() { Interval = 15 }; // run a little faster than 60hz
            UpdateTimer.Tick += (sender, args) => UpdateScript();
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
            scriptCleanup();
        }

        protected void UpdateScript()
        {
            // Disable timer, to wait for execution of this iteration to finish
            UpdateTimer.Enabled = false;

            // this is ugly, fix eventually!
            if (Settings.ScriptPath != OldScriptPath || DoReload)
            {
                try
                {
                    DoReload = false;
                    OldScriptPath = Settings.ScriptPath;
                    if (string.IsNullOrEmpty(Settings.ScriptPath))
                    {
                        scriptCleanup();
                        FSWatcher.EnableRaisingEvents = false;
                    }
                    else
                    {
                        loadScript();
                    }
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
                    Script.RunUpdate(State);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
            UpdateTimer.Enabled = true;
        }

        private void loadScript()
        {
            scriptCleanup();

            FSWatcher.Path = Path.GetDirectoryName(Settings.ScriptPath);
            FSWatcher.Filter = Path.GetFileName(Settings.ScriptPath);
            FSWatcher.EnableRaisingEvents = true;

            // New script
            Script = ASLParser.Parse(File.ReadAllText(Settings.ScriptPath));

            Script.RefreshRateChanged += Script_RefreshRateChanged;
            Script_RefreshRateChanged(this, Script.RefreshRate);

            Script.GameVersionChanged += Script_GameVersionChanged;
            Settings.SetGameVersion(null);

            // Give custom ASL settings to GUI, which populates the list and
            // stores the ASLSetting objects which are shared between the GUI
            // and ASLScript
            Settings.SetASLSettings(Script.RunStartup(State));
        }

        private void scriptCleanup()
        {
            if (Script != null)
            {
                Script.RefreshRateChanged -= Script_RefreshRateChanged;
                Script.GameVersionChanged -= Script_GameVersionChanged;
                Script.RunShutdown(State);
                Settings.SetGameVersion(null);
                Settings.SetASLSettings(new ASLSettings());
                Script = null;
            }
        }

        private void Script_RefreshRateChanged(object sender, double e)
        {
            UpdateTimer.Interval = (int)Math.Round(1000 / e);
        }

        private void Script_GameVersionChanged(object sender, string version)
        {
            Settings.SetGameVersion(version);
        }
    }
}
