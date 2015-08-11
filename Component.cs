#define GAME_TIME

using LiveSplit.ASL;
using LiveSplit.Model;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LiveSplit.UI.Components
{
    class Component : LogicComponent
    {
        public ComponentSettings Settings { get; set; }

        public override string ComponentName
        {
            get { return "Scriptable Auto Splitter"; }
        }

        protected String OldScriptPath { get; set; }
        protected FileSystemWatcher FSWatcher { get; set; }
        protected bool DoReload { get; set; }
        protected CancellationTokenSource UpdateCancel { get; set; }
        protected Task UpdateTask { get; set; }

        public bool Refresh { get; set; }

        public ASLScript Script { get; set; }

        public Component(LiveSplitState state, String scriptPath): this(state)
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
            UpdateCancel = new CancellationTokenSource();
            UpdateTask = Task.Run(() => ScriptUpdateLoop(state), UpdateCancel.Token);
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {

        }

        public override System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public override System.Windows.Forms.Control GetSettingsControl(LayoutMode mode)
        {
            return Settings;
        }

        public override void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public override void Dispose()
        {
            UpdateCancel.Cancel();
            UpdateTask.Wait();

            if (FSWatcher != null)
                FSWatcher.Dispose();
            if (UpdateCancel != null)
                UpdateCancel.Dispose();
            if (UpdateTask != null)
                UpdateTask.Dispose();
        }

        protected void ScriptUpdateLoop(LiveSplitState state)
        {
            while (!UpdateCancel.IsCancellationRequested)
            {
                // this is ugly, fix eventually!
                if (Settings.ScriptPath != OldScriptPath && !String.IsNullOrEmpty(Settings.ScriptPath) || DoReload)
                {
                    Script = ASLParser.Parse(File.ReadAllText(Settings.ScriptPath));
                    OldScriptPath = Settings.ScriptPath;
                    FSWatcher.Path = Path.GetDirectoryName(Settings.ScriptPath);
                    FSWatcher.Filter = Path.GetFileName(Settings.ScriptPath);
                    FSWatcher.EnableRaisingEvents = true;
                    DoReload = false;
                }

                if (Script != null)
                    Script.Update(state);

                Thread.Sleep(TimeSpan.FromMilliseconds(1000 / 64.0f)); // update slightly faster than 60hz
            }
        }
    }
}
