using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;

[assembly: ComponentFactory(typeof(Factory))]

namespace LiveSplit.UI.Components
{
    public class Factory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "Scriptable Auto Splitter"; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new Component();
        }

        public IComponent Create(LiveSplitState state, String script)
        {
            return new Component(script);
        }

        public string UpdateName
        {
            get { return ComponentName; }
        }

        public string XMLURL
        {
#if RELEASE_CANDIDATE
#else
            get { return "http://livesplit.org/update/Components/update.LiveSplit.ScriptableAutoSplit.xml"; }
#endif
        }

        public string UpdateURL
        {
#if RELEASE_CANDIDATE
#else
            get { return "http://livesplit.org/update/"; }
#endif
        }

        public Version Version
        {
            get { return Version.Parse("1.1.2"); }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.Control; }
        }

        public string Description
        {
            get { return "Allows scripts written in the ASL language to define the splitting behaviour."; }
        }
    }
}
