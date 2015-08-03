#define GAME_TIME

using LiveSplit.ASL;
using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace LiveSplit.UI.Components
{
    class Component : IComponent
    {
        public ComponentSettings Settings { get; set; }

        public string ComponentName
        {
            get { return "Scriptable Auto Splitter"; }
        }

        public float PaddingBottom { get { return 0; } }
        public float PaddingTop { get { return 0; } }
        public float PaddingLeft { get { return 0; } }
        public float PaddingRight { get { return 0; } }

        protected String OldScriptPath { get; set; }
        protected FileSystemWatcher FSWatcher { get; set; }
        protected bool DoReload { get; set; }

        public bool Refresh { get; set; }

        public IDictionary<string, Action> ContextMenuControls { get; protected set; }

        public ASLScript Script { get; set; }

        public Component(String scriptPath)
        {
            Settings = new ComponentSettings()
            {
                ScriptPath = scriptPath
            };
        }

        public Component()
        {
            Settings = new ComponentSettings();
            FSWatcher = new FileSystemWatcher();
            FSWatcher.Changed += (sender, args) => DoReload = true;
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if ((Settings.ScriptPath != OldScriptPath && !String.IsNullOrEmpty(Settings.ScriptPath)) || DoReload)
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
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
        }

        public float VerticalHeight
        {
            get { return 0; }
        }

        public float MinimumWidth
        {
            get { return 0; }
        }

        public float HorizontalWidth
        {
            get { return 0; }
        }

        public float MinimumHeight
        {
            get { return 0; }
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public System.Windows.Forms.Control GetSettingsControl(UI.LayoutMode mode)
        {
            return Settings;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public void Dispose()
        {
            if (FSWatcher != null)
                FSWatcher.Dispose();
        }
    }
}
