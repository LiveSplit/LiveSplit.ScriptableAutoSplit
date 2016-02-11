using LiveSplit.Options;
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
        private Dictionary<object, ASL.ASLSetting> _basicSettings;

        public ComponentSettings()
        {
            InitializeComponent();

            ScriptPath = "";
            _basicSettings = new Dictionary<object, ASL.ASLSetting>();

            txtScriptPath.DataBindings.Add("Text", this, "ScriptPath", false, DataSourceUpdateMode.OnPropertyChanged);

            SetGameVersion(null);
            updateCustomSettingsVisibility();
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "Version", "1.5"));
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "ScriptPath", ScriptPath));
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "Start", startCheckbox.Checked));
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "Reset", resetCheckbox.Checked));
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "Split", splitCheckbox.Checked));
            appendCustomSettingsToXml(document, settingsNode);
            return settingsNode;
        }

        // Loads the settings of this component from Xml. This might happen more than once
        // (e.g. when the settings dialog is cancelled to restore previous settings).
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
        }

        // Sets the custom settings defined in the ASL script. Populates the CheckedListBox.
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

            initBasicSettings(settings);
        }

        private void initBasicSettings(ASL.ASLSettings settings)
        {
            _basicSettings.Clear();
            initBasicSetting(settings, startCheckbox, "start");
            initBasicSetting(settings, resetCheckbox, "reset");
            initBasicSetting(settings, splitCheckbox, "split");
        }

        private void initBasicSetting(ASL.ASLSettings settings, CheckBox checkbox, string name)
        {
            if (settings.MethodPresent(name))
            {
                ASL.ASLSetting setting = settings.MethodSettings[name];
                _basicSettings.Add(checkbox, setting);
                checkbox.Enabled = true;
                setting.Value = checkbox.Checked;
            }
            else
            {
                checkbox.Enabled = false;
                checkbox.Checked = false;
            }
        }

        private void updateCustomSettingsVisibility()
        {
            bool show = _defaultValues != null && _defaultValues.Count > 0;
            customSettingsList.Visible = show;
            resetToDefaultButton.Visible = show;
            checkAllButton.Visible = show;
            uncheckAllButton.Visible = show;
            customSettingsLabel.Visible = show;
        }

        // Updates the checked state of the CheckedListBox items based on the
        // settingValues parameter. This will implicitly also update the value
        // in the associated ASLSetting objects, which are shared with the ASL
        // script.
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
                // In case there are other setting types in the future
                XmlAttribute type = SettingsHelper.ToAttribute(document, "type", "bool");
                element.Attributes.Append(id);
                element.Attributes.Append(type);
                aslParent.AppendChild(element);
            }
            parent.AppendChild(aslParent);
        }

        // Parses the ASLSettings from the given XML Element (which should be the Settings-element
        // from the component settings). Stores them in a Dictionary for later usage.
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
                        string type = element.Attributes["type"].Value;
                        if (id != null && type == "bool")
                        {
                            bool value = SettingsHelper.ParseBool(element);
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
            gameVersionLabel.Text = version != null ? "Game Version: " + version : "";
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
            // Update value in the ASLSetting object, which also changes it in the ASL script
            setting.Value = e.NewValue == CheckState.Checked;
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
            if (_basicSettings.ContainsKey(sender))
            {
                _basicSettings[sender].Value = ((CheckBox)sender).Checked;
            }
        }

    }
}
