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
        private Dictionary<object, ASL.ASLSetting> _methodSettings;

        public ComponentSettings()
        {
            InitializeComponent();

            ScriptPath = "";
            _methodSettings = new Dictionary<object, ASL.ASLSetting>();

            txtScriptPath.DataBindings.Add("Text", this, "ScriptPath", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "Version", "1.4"));
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "ScriptPath", ScriptPath));
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "Start", startCheckbox.Checked));
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "Reset", resetCheckbox.Checked));
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "Split", splitCheckbox.Checked));
            appendCustomSettingsToXml(document, settingsNode);
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
                startCheckbox.Checked = SettingsHelper.ParseBool(element["Start"], true);
                resetCheckbox.Checked = SettingsHelper.ParseBool(element["Reset"], true);
                splitCheckbox.Checked = SettingsHelper.ParseBool(element["Split"], true);
                parseCustomSettingsFromXml(element);
            }
            Console.WriteLine("SetSettings #####"+element["Start"]);
        }

        /// <summary>
        /// Sets the custom settings defined in the ASL script. Populates the CheckedListBox.
        /// </summary>
        /// <param name="settings"></param>
        public void SetASLSettings(ASL.ASLSettings settings)
        {
            customSettingsList.Items.Clear();
            Dictionary<string, bool> values = new Dictionary<string, bool>();
            foreach (var setting in settings.OrderedSettings)
            {
                customSettingsList.Items.Add(setting, setting.Value);
                values.Add(setting.Name, setting.Value);
            }
            _defaultValues = values;
            // Update from settings (in case settings are already loaded, which should be the case)
            updateItemsInList(_lastLoadedFromSettings);
            updateCustomSettingsVisibility();

            updateBasicSettings(settings);
        }

        private void updateBasicSettings(ASL.ASLSettings settings)
        {
            _methodSettings.Clear();
            updateBasicSetting(settings, startCheckbox, "start");
            updateBasicSetting(settings, resetCheckbox, "reset");
            updateBasicSetting(settings, splitCheckbox, "split");
        }

        private void updateBasicSetting(ASL.ASLSettings settings, CheckBox checkbox, string name)
        {
            //checkbox.DataBindings.Clear();
            if (settings.MethodPresent(name))
            {
                ASL.ASLSetting setting = settings.MethodSettings[name];
                //checkbox.DataBindings.Add("Checked", setting, "Value");
                _methodSettings.Add(checkbox, setting);
                checkbox.Enabled = true;
                setting.Value = checkbox.Checked;
            } else
            {
                checkbox.Enabled = false;
                checkbox.Checked = false;
            }
        }

        private void updateCustomSettingsVisibility()
        {
            bool show = _defaultValues.Count > 0;
            customSettingsList.Visible = show;
            resetToDefaultButton.Visible = show;
            checkAllButton.Visible = show;
            uncheckAllButton.Visible = show;
            customSettingsLabel.Visible = show;
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
            for (int i = 0; i < customSettingsList.Items.Count; i++)
            {
                var setting = (ASL.ASLSetting)customSettingsList.Items[i];
                string id = setting.Name;
                bool value = setting.Value;
                if (settingValues.ContainsKey(id))
                {
                    value = settingValues[id];
                }
                customSettingsList.SetItemChecked(i, value);
            }
        }

        private void appendCustomSettingsToXml(XmlDocument document, XmlNode parent)
        {
            XmlElement aslParent = document.CreateElement("CustomSettings");
            foreach (var item in customSettingsList.Items)
            {
                var setting = (ASL.ASLSetting)item;
                XmlElement element = SettingsHelper.ToElement(document, "Setting", setting.Value);
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
        private void parseCustomSettingsFromXml(XmlElement data)
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            XmlElement customSettingsNode = data["CustomSettings"];
            if (customSettingsNode != null && customSettingsNode.HasChildNodes)
            {
                foreach (XmlElement element in customSettingsNode.ChildNodes)
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

        public void SetGameVersion(string version)
        {
            gameVersion.Text = version != null ? "Game Version: " + version : "";
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

        private void customSettingsList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ASL.ASLSetting setting = (ASL.ASLSetting)customSettingsList.Items[e.Index];
            setting.Value = e.NewValue == CheckState.Checked;
            //Console.WriteLine(((CheckedListBox)sender).Items[e.Index] + " " + e.NewValue);
        }

        private void checkAllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < customSettingsList.Items.Count; i++)
            {
                customSettingsList.SetItemChecked(i, true);
            }
        }

        private void uncheckAllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < customSettingsList.Items.Count; i++)
            {
                customSettingsList.SetItemChecked(i, false);
            }
        }

        private void resetToDefaultButton_Click(object sender, EventArgs e)
        {
            updateItemsInList(_defaultValues);
        }

        private void methodCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (_methodSettings.ContainsKey(sender))
            {
                _methodSettings[sender].Value = ((CheckBox)sender).Checked;
            }
        }

    }
}
