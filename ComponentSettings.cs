using LiveSplit.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class ComponentSettings : UserControl
    {
        public string ScriptPath { get; set; }

        private Dictionary<string, bool> _customSettingsFromXml;
        private Dictionary<string, bool> _defaultValues;
        private Dictionary<string, CheckBox> _basicSettings;
        private Dictionary<string, bool> _basicSettingsFromXml;

        public ComponentSettings()
        {
            InitializeComponent();

            ScriptPath = "";

            txtScriptPath.DataBindings.Add("Text", this, "ScriptPath", false, DataSourceUpdateMode.OnPropertyChanged);

            SetGameVersion(null);
            updateCustomSettingsVisibility();

            _basicSettings = new Dictionary<string, CheckBox>();
            // Capitalized names for saving it in XML.
            _basicSettings["Start"] = checkboxStart;
            _basicSettings["Reset"] = checkboxReset;
            _basicSettings["Split"] = checkboxSplit;
            _basicSettingsFromXml = new Dictionary<string, bool>();
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "Version", "1.5"));
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "ScriptPath", ScriptPath));
            appendBasicSettingsToXml(document, settingsNode);
            appendCustomSettingsToXml(document, settingsNode);
            return settingsNode;
        }

        // Loads the settings of this component from Xml. This might happen more than once
        // (e.g. when the settings dialog is cancelled, to restore previous settings).
        public void SetSettings(XmlNode settings)
        {
            var element = (XmlElement)settings;
            if (!element.IsEmpty)
            {
                ScriptPath = SettingsHelper.ParseString(element["ScriptPath"], string.Empty);
                parseBasicSettingsFromXml(element);
                parseCustomSettingsFromXml(element);
            }
        }

        private void appendBasicSettingsToXml(XmlDocument document, XmlNode settingsNode)
        {
            foreach (var item in _basicSettings)
            {
                settingsNode.AppendChild(SettingsHelper.ToElement(document, item.Key, item.Value.Checked));
            }
        }

        private void appendCustomSettingsToXml(XmlDocument document, XmlNode parent)
        {
            XmlElement aslParent = document.CreateElement("CustomSettings");
            foreach (ASL.ASLSetting setting in getListOfCustomSettings())
            {
                XmlElement element = SettingsHelper.ToElement(document, "Setting", setting.Value);
                XmlAttribute id = SettingsHelper.ToAttribute(document, "id", setting.Id);
                // In case there are other setting types in the future
                XmlAttribute type = SettingsHelper.ToAttribute(document, "type", "bool");
                element.Attributes.Append(id);
                element.Attributes.Append(type);
                aslParent.AppendChild(element);
            }
            parent.AppendChild(aslParent);
        }

        /// <summary>
        /// Gets a flat list of all custom settings from the settings tree.
        /// </summary>
        /// 
        private List<ASL.ASLSetting> getListOfCustomSettings()
        {
            List<ASL.ASLSetting> list = new List<ASL.ASLSetting>();
            updateNodesInTree(node => {
                list.Add((ASL.ASLSetting)node.Tag);
                return true;
            }, treeCustomSettings.Nodes);
            return list;
        }

        private void parseBasicSettingsFromXml(XmlElement element)
        {
            foreach (var item in _basicSettings)
            {
                bool value = SettingsHelper.ParseBool(element[item.Key], true);
                item.Value.Checked = value;
                _basicSettingsFromXml[item.Key.ToLower()] = value;
            }
        }

        /// <summary>
        /// Parses custom settings, stores them and updates the checked state of already added tree nodes.
        /// </summary>
        /// 
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
            _customSettingsFromXml = result;
            // Update from settings when loaded (in case the list is already populated)
            updateNodeCheckedState(_customSettingsFromXml);
        }

        public void SetGameVersion(string version)
        {
            labelGameVersion.Text = version != null ? "Game Version: " + version : "";
        }

        /// <summary>
        /// Populates the component with the settings defined in the ASL script.
        /// </summary>
        /// 
        public void SetASLSettings(ASL.ASLSettings settings)
        {
            treeCustomSettings.BeginUpdate();
            treeCustomSettings.Nodes.Clear();

            Dictionary<string, bool> values = new Dictionary<string, bool>();

            // Store temporary for easier lookup of parent nodes
            Dictionary<string, TreeNode> flat = new Dictionary<string, TreeNode>();

            foreach (var setting in settings.OrderedSettings)
            {
                TreeNode node = new TreeNode(setting.Label);
                node.Tag = setting;
                node.Checked = setting.Value;
                if (setting.Parent == null)
                {
                    treeCustomSettings.Nodes.Add(node);
                }
                else if (flat.ContainsKey(setting.Parent))
                {
                    flat[setting.Parent].Nodes.Add(node);
                    flat[setting.Parent].ContextMenuStrip = treeContextMenu;
                } 
                flat.Add(setting.Id, node);
                values.Add(setting.Id, setting.Value);
            }
            _defaultValues = values;

            // Update from XML (in case settings are already loaded, which should be the case)
            updateNodeCheckedState(_customSettingsFromXml);

            treeCustomSettings.ExpandAll();
            treeCustomSettings.EndUpdate();

            updateCustomSettingsVisibility();
            initBasicSettings(settings);
        }

        private void initBasicSettings(ASL.ASLSettings settings)
        {
            foreach (var item in _basicSettings)
            {
                string name = item.Key.ToLower();
                CheckBox checkbox = item.Value;
                if (settings.IsBasicSettingPresent(name))
                {
                    ASL.ASLSetting setting = settings.BasicSettings[name];
                    checkbox.Enabled = true;
                    checkbox.Tag = setting;
                    bool value = true;
                    if (_basicSettingsFromXml.ContainsKey(name))
                    {
                        value = _basicSettingsFromXml[name];
                    }
                    checkbox.Checked = value;
                    setting.Value = value;
                }
                else
                {
                    checkbox.Tag = null;
                    checkbox.Enabled = false;
                    checkbox.Checked = false;
                }
            }
        }

        private void updateCustomSettingsVisibility()
        {
            bool show = treeCustomSettings.GetNodeCount(false) > 0;
            treeCustomSettings.Visible = show;
            btnResetToDefault.Visible = show;
            btnCheckAll.Visible = show;
            btnUncheckAll.Visible = show;
            labelCustomSettings.Visible = show;
        }

        /// <summary>
        /// Update the checked state of all given nodes and their childnodes
        /// based on a dictionary of setting values.
        /// </summary>
        /// 
        private void updateNodeCheckedState(Dictionary<string, bool> settingValues,
            TreeNodeCollection nodes = null)
        {
            if (settingValues == null)
            {
                return;
            }
            updateNodeCheckedState(setting => {
                string id = setting.Id;
                if (settingValues.ContainsKey(id))
                {
                    return settingValues[id];
                }
                return setting.Value;
            }, nodes);
        }

        /// <summary>
        /// Generic update on all given nodes and their childnodes, ignoring childnodes for
        /// nodes where the Func returns false.
        /// </summary>
        /// 
        private void updateNodesInTree(Func<TreeNode, bool> func, TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                bool includeChildNodes = func(node);
                if (includeChildNodes)
                {
                    updateNodesInTree(func, node.Nodes);
                }
            }
        }

        /// <summary>
        /// Update the checked state of all given nodes and their childnodes based on the return
        /// value of the given Func.
        /// </summary>
        /// <param name="nodes">If nodes is null, all nodes of the custom settings tree are affected.</param>
        /// 
        private void updateNodeCheckedState(Func<ASL.ASLSetting, bool> func, TreeNodeCollection nodes = null)
        {
            if (nodes == null)
            {
                nodes = treeCustomSettings.Nodes;
            }
            updateNodesInTree(node =>
            {
                ASL.ASLSetting setting = (ASL.ASLSetting)node.Tag;
                bool check = func(setting);
                if (node.Checked != check)
                {
                    node.Checked = check;
                }
                return true;
            }, nodes);
        }


        // Events

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

        private void methodCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            ASL.ASLSetting setting = (ASL.ASLSetting)checkbox.Tag;
            if (setting != null)
            {
                setting.Value = checkbox.Checked;
            }
        }

        private void settingsTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // Update value in the ASLSetting object, which also changes it in the ASL script
            ASL.ASLSetting setting = (ASL.ASLSetting)e.Node.Tag;
            setting.Value = e.Node.Checked;

            // Only change color of childnodes if this node isn't already grayed out
            if (e.Node.ForeColor != SystemColors.GrayText) 
            {
                updateNodesInTree(node => {
                    node.ForeColor = e.Node.Checked ? SystemColors.WindowText : SystemColors.GrayText;
                    return node.Checked;
                },
                e.Node.Nodes);
            }
        }


        // Custom Settings Button Events

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            updateNodeCheckedState(id => true);
        }

        private void btnUncheckAll_Click(object sender, EventArgs e)
        {
            updateNodeCheckedState(id => false);
        }

        private void btnResetToDefault_Click(object sender, EventArgs e)
        {
            updateNodeCheckedState(_defaultValues);
        }


        // Custom Settings Context Menu Events

        private void settingsTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Select clicked node (not only with left-click) for use with context menu
            treeCustomSettings.SelectedNode = e.Node;
        }

        private void cmiCheckBranch_Click(object sender, EventArgs e)
        {
            updateNodeCheckedState(i => true, treeCustomSettings.SelectedNode.Nodes);
        }

        private void cmiUncheckBranch_Click(object sender, EventArgs e)
        {
            updateNodeCheckedState(i => false, treeCustomSettings.SelectedNode.Nodes);
        }

        private void cmiResetBranchToDefault_Click(object sender, EventArgs e)
        {
            updateNodeCheckedState(_defaultValues, treeCustomSettings.SelectedNode.Nodes);
        }
    }

    /// <summary>
    /// TreeView with fixed double-clicking on checkboxes.
    /// </summary>
    /// 
    /// See also:
    /// http://stackoverflow.com/questions/17356976/treeview-with-checkboxes-not-processing-clicks-correctly
    /// http://stackoverflow.com/questions/14647216/c-sharp-treeview-ignore-double-click-only-at-checkbox
    class NewTreeView : TreeView
    {
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x203) // identified double click
            {
                var localPos = PointToClient(Cursor.Position);
                var hitTestInfo = HitTest(localPos);
                if (hitTestInfo.Location == TreeViewHitTestLocations.StateImage)
                {
                    m.Msg = 0x201; // if checkbox was clicked, turn into single click
                }
                base.WndProc(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

    }

}
