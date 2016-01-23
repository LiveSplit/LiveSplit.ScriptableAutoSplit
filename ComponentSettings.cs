using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class ComponentSettings : UserControl
    {
        public string ScriptPath { get; set; }

        private Dictionary<string, bool> _lastLoadedFromSettings;
        private Dictionary<string, bool> _defaultValues;

        public ComponentSettings()
        {
            InitializeComponent();

            ScriptPath = "";

            txtScriptPath.DataBindings.Add("Text", this, "ScriptPath", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public void SetGameVersion(string version)
        {
            gameVersion.Text = version != null ? "Detected Game Version: " + version : "";
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "Version", "1.4"));
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "ScriptPath", ScriptPath));
            appendASLSettingsToXml(document, settingsNode);
            return settingsNode;
        }

        /// <summary>
        /// Loads the settings of this component from Xml. This might happen more than once
        /// (e.g. when the settings dialog is cancelled to restore previous settings).
        /// </summary>
        /// <param name="settings"></param>
        public void SetSettings(XmlNode settings)
        {
            var element = (XmlElement)settings;
            if (!element.IsEmpty)
            {
                ScriptPath = SettingsHelper.ParseString(element["ScriptPath"], string.Empty);
                parseASLSettingsFromXml(element);
            }
            Console.WriteLine("SetSEttings");
        }

        /// <summary>
        /// Sets the custom settings defined in the ASL script. Populates the CheckedListBox.
        /// </summary>
        /// <param name="settings"></param>
        public void SetASLSettings(ASL.ASLSettings settings)
        {
            aslSettings.Items.Clear();
            Dictionary<string, bool> values = new Dictionary<string, bool>();
            foreach (var item in settings.Settings)
            {
                ASL.ASLSetting setting = item.Value;
                aslSettings.Items.Add(setting, setting.Enabled);
                values.Add(setting.Name, setting.Enabled);
            }
            _defaultValues = values;
            // Update from settings (in case settings are already loaded, which should be the case)
            updateItemsInList(_lastLoadedFromSettings);
            updateOptionsVisibility();
        }

        private void updateOptionsVisibility()
        {
            bool show = _defaultValues.Count > 0;
            aslSettings.Visible = show;
            resetToDefaultButton.Visible = show;
            checkAllButton.Visible = show;
            uncheckAllButton.Visible = show;
            optionsLabel.Visible = show;
        }

        private void appendASLSettingsToXml(XmlDocument document, XmlNode parent)
        {
            XmlElement aslParent = document.CreateElement("ASLSettings");
            foreach (var item in aslSettings.Items)
            {
                var setting = (ASL.ASLSetting)item;
                XmlElement element = SettingsHelper.ToElement(document, "Setting", setting.Enabled);
                XmlAttribute id = SettingsHelper.ToAttribute(document, "id", setting.Name);
                element.Attributes.Append(id);
                aslParent.AppendChild(element);
            }
            parent.AppendChild(aslParent);
        }

        /// <summary>
        /// Parses the ASLSettings from the given XML Element (which should be the Settings-element
        /// from the component settings). Stores them in a Dictionary for later usage.
        /// </summary>
        /// <param name="data"></param>
        private void parseASLSettingsFromXml(XmlElement data)
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            XmlElement aslSettingsNode = data["ASLSettings"];
            if (aslSettingsNode != null && aslSettingsNode.HasChildNodes)
            {
                foreach (XmlElement element in aslSettingsNode.ChildNodes)
                {
                    if (element.Name == "Setting")
                    {
                        string id = element.Attributes["id"].Value;
                        bool value = SettingsHelper.ParseBool(element);
                        if (id != null)
                        {
                            result.Add(id, value);
                        }
                    }
                }
            }
            _lastLoadedFromSettings = result;
            // Update from settings when loaded (in case the list is already populated)
            updateItemsInList(_lastLoadedFromSettings);
        }

        /// <summary>
        /// Updates the values of the CheckedListBox entries based on what was last loaded from
        /// the settings. This will implicitly also update the value in the ASLSetting
        /// object in the list.
        /// </summary>
        private void updateItemsInList(Dictionary<string, bool> settingValues)
        {
            if (settingValues == null)
            {
                return;
            }
            for (int i = 0; i < aslSettings.Items.Count; i++)
            {
                var setting = (ASL.ASLSetting)aslSettings.Items[i];
                string id = setting.Name;
                bool value = setting.Enabled;
                if (settingValues.ContainsKey(id))
                {
                    value = settingValues[id];
                }
                aslSettings.SetItemChecked(i, value);
            }
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                FileName = ScriptPath,
                Filter = "Auto Split Script (*.asl)|*.asl|All Files (*.*)|*.*"
            };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
                ScriptPath = txtScriptPath.Text = dialog.FileName;
        }

        private void aslSettings_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ASL.ASLSetting setting = (ASL.ASLSetting)aslSettings.Items[e.Index];
            setting.Enabled = e.NewValue == CheckState.Checked;
            Console.WriteLine(((CheckedListBox)sender).Items[e.Index] + " " + e.NewValue);
            

            //ASL.ASLSetting selected = (ASL.ASLSetting)aslSettings.SelectedItem;
            //if (selected != null)
            //{
            //    selected.Enabled = e.NewValue == CheckState.Checked;
            //    Console.WriteLine(((CheckedListBox)sender).Items[e.Index] + " " + e.NewValue);
            //}
        }

        private void checkAllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < aslSettings.Items.Count; i++)
            {
                aslSettings.SetItemChecked(i, true);
            }
        }

        private void uncheckAllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < aslSettings.Items.Count; i++)
            {
                aslSettings.SetItemChecked(i, false);
            }
        }

        private void resetToDefaultButton_Click(object sender, EventArgs e)
        {
            updateItemsInList(_defaultValues);
        }
    }
}
