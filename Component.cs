#define GAME_TIME

using LiveSplit.ASL;
using LiveSplit.Model;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Options;
using System.Diagnostics;

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
            scriptCleanup();

            if (FSWatcher != null)
                FSWatcher.Dispose();
            if (UpdateTimer != null)
                UpdateTimer.Dispose();
        }

        protected void UpdateScript()
        {
            // Disable timer, to wait for execution of this iteration to
            // finish. This can be useful if blocking operations like
            // showing a message window are used.
            UpdateTimer.Enabled = false;

            // this is ugly, fix eventually!
            if (Settings.ScriptPath != OldScriptPath || DoReload)
            {
                try
                {
                    DoReload = false;
                    OldScriptPath = Settings.ScriptPath;

                    scriptCleanup();
                    if (string.IsNullOrEmpty(Settings.ScriptPath))
                    {
                        // Only disable file watcher if script path changed to empty
                        // (otherwise detecting file changes may still be wanted)
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
            Trace.WriteLine("[ASL] Loading new script: "+Settings.ScriptPath);

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
            try
            {
                ASL.ASLSettings settingsFromASL = Script.RunStartup(State);
                Settings.SetASLSettings(settingsFromASL);
            }
            catch (Exception ex)
            {
                // Script already created, but startup failed, so clean up again
                Log.Error(ex);
                scriptCleanup();
            }
        }

        private void scriptCleanup()
        {
            if (Script != null)
            {
                try
                {
                    Script.RefreshRateChanged -= Script_RefreshRateChanged;
                    Script.GameVersionChanged -= Script_GameVersionChanged;
                    Settings.SetGameVersion(null);
                    Settings.SetASLSettings(new ASLSettings());
                    Script.RunShutdown(State);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
                finally
                {
                    // Script should no longer be used, even in case of error
                    // (which the ASL shutdown method may contain)
                    Script = null;
                }
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
