using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.ASL;

namespace LiveSplit.UI.Components
{
    public partial class ComponentSettings : UserControl
    {
        public string ScriptPath { get; set; }

        // Save the state of settings independant of actual ASLSetting
        // objects (which are specific to the loaded ASL Script instance).
        //
        // This is used to restore the correct state when the ASL
        // Script is first loaded or reloaded.
        private Dictionary<string, bool> _basic_settings_state;
        private Dictionary<string, bool> _custom_settings_state;

        // For resetting to default values. This could also be handled
        // in a field in ASLSetting, but this dict can be used with the
        // same method to update the tree as _customSettingsState.
        private Dictionary<string, bool> _default_values;

        private Dictionary<string, CheckBox> _basic_settings;

        public ComponentSettings()
        {
            InitializeComponent();

            ScriptPath = string.Empty;

            this.txtScriptPath.DataBindings.Add("Text", this, "ScriptPath", false,
                DataSourceUpdateMode.OnPropertyChanged);

            SetGameVersion(null);
            UpdateCustomSettingsVisibility();

            _basic_settings = new Dictionary<string, CheckBox> {
                // Capitalized names for saving it in XML.
                ["Start"] = checkboxStart,
                ["Reset"] = checkboxReset,
                ["Split"] = checkboxSplit
            };

            _basic_settings_state = new Dictionary<string, bool>();
            _custom_settings_state = new Dictionary<string, bool>();
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            XmlElement settings_node = document.CreateElement("Settings");

            settings_node.AppendChild(SettingsHelper.ToElement(document, "Version", "1.5"));
            settings_node.AppendChild(SettingsHelper.ToElement(document, "ScriptPath", ScriptPath));
            AppendBasicSettingsToXml(document, settings_node);
            AppendCustomSettingsToXml(document, settings_node);

            return settings_node;
        }

        // Loads the settings of this component from Xml. This might happen more than once
        // (e.g. when the settings dialog is cancelled, to restore previous settings).
        public void SetSettings(XmlNode settings)
        {
            var element = (XmlElement)settings;
            if (!element.IsEmpty)
            {
                ScriptPath = SettingsHelper.ParseString(element["ScriptPath"], string.Empty);
                ParseBasicSettingsFromXml(element);
                ParseCustomSettingsFromXml(element);
            }
        }

        public void SetGameVersion(string version)
        {
            this.lblGameVersion.Text = string.IsNullOrEmpty(version) ? "" : "Game Version: " + version;
        }

        /// <summary>
        /// Populates the component with the settings defined in the ASL script.
        /// </summary>
        /// 
        public void SetASLSettings(ASLSettings settings)
        {
            this.treeCustomSettings.BeginUpdate();
            this.treeCustomSettings.Nodes.Clear();

            var values = new Dictionary<string, bool>();

            // Store temporary for easier lookup of parent nodes
            var flat = new Dictionary<string, TreeNode>();

            foreach (ASLSetting setting in settings.OrderedSettings)
            {
                var node = new TreeNode(setting.Label) {
                    Tag = setting,
                    Checked = setting.Value,
                    ContextMenuStrip = this.treeContextMenu2,
                    ToolTipText = setting.ToolTip
                };

                if (setting.Parent == null)
                {
                    this.treeCustomSettings.Nodes.Add(node);
                }
                else if (flat.ContainsKey(setting.Parent))
                {
                    flat[setting.Parent].Nodes.Add(node);
                    flat[setting.Parent].ContextMenuStrip = this.treeContextMenu;
                }

                flat.Add(setting.Id, node);
                values.Add(setting.Id, setting.Value);
            }

            // Gray out deactivated nodes after all have been added
            foreach (var item in flat) {
                if (!item.Value.Checked)
                {
                    UpdateGrayedOut(item.Value);
                }
            }

            _default_values = values;

            // Update from saved state (from XML or stored between script reloads)
            UpdateNodeCheckedState(_custom_settings_state);

            treeCustomSettings.ExpandAll();
            treeCustomSettings.EndUpdate();

            // Scroll up to the top
            if (this.treeCustomSettings.Nodes.Count > 0)
                this.treeCustomSettings.Nodes[0].EnsureVisible();

            UpdateCustomSettingsVisibility();
            InitBasicSettings(settings);
        }


        private void AppendBasicSettingsToXml(XmlDocument document, XmlNode settings_node)
        {
            foreach (var item in _basic_settings)
            {
                settings_node.AppendChild(SettingsHelper.ToElement(document, item.Key, item.Value.Checked));
            }
        }

        private void AppendCustomSettingsToXml(XmlDocument document, XmlNode parent)
        {
            XmlElement asl_parent = document.CreateElement("CustomSettings");

            foreach (ASLSetting setting in GetListOfCustomSettings())
            {
                XmlElement element = SettingsHelper.ToElement(document, "Setting", setting.Value);
                XmlAttribute id = SettingsHelper.ToAttribute(document, "id", setting.Id);
                // In case there are other setting types in the future
                XmlAttribute type = SettingsHelper.ToAttribute(document, "type", "bool");

                element.Attributes.Append(id);
                element.Attributes.Append(type);
                asl_parent.AppendChild(element);
            }

            parent.AppendChild(asl_parent);
        }

        /// <summary>
        /// Gets a flat list of all custom settings from the settings tree.
        /// </summary>
        ///
        private List<ASLSetting> GetListOfCustomSettings()
        {
            var list = new List<ASLSetting>();

            UpdateNodesInTree(node => {
                list.Add((ASLSetting)node.Tag);
                return true;
            }, this.treeCustomSettings.Nodes);

            return list;
        }

        private void ParseBasicSettingsFromXml(XmlElement element)
        {
            foreach (var item in _basic_settings)
            {
                bool value = SettingsHelper.ParseBool(element[item.Key], true);
                item.Value.Checked = value;
                _basic_settings_state[item.Key.ToLower()] = value;
            }
        }

        /// <summary>
        /// Parses custom settings, stores them and updates the checked state of already added tree nodes.
        /// </summary>
        /// 
        private void ParseCustomSettingsFromXml(XmlElement data)
        {
            var result = new Dictionary<string, bool>();
            XmlElement custom_settings_node = data["CustomSettings"];

            if (custom_settings_node != null && custom_settings_node.HasChildNodes)
            {
                foreach (XmlElement element in custom_settings_node.ChildNodes)
                {
                    if (element.Name != "Setting")
                        continue;

                    string id = element.Attributes["id"].Value;
                    string type = element.Attributes["type"].Value;

                    if (id != null && type == "bool")
                    {
                        bool value = SettingsHelper.ParseBool(element);
                        result.Add(id, value);
                    }
                }
            }

            _custom_settings_state = result;
            // Update tree with loaded state (in case the tree is already populated)
            UpdateNodeCheckedState(_custom_settings_state);
        }

        private void InitBasicSettings(ASLSettings settings)
        {
            foreach (var item in _basic_settings)
            {
                string name = item.Key.ToLower();
                CheckBox checkbox = item.Value;

                if (settings.IsBasicSettingPresent(name))
                {
                    ASLSetting setting = settings.BasicSettings[name];
                    checkbox.Enabled = true;
                    checkbox.Tag = setting;
                    var value = true;

                    if (_basic_settings_state.ContainsKey(name))
                        value = _basic_settings_state[name];

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

        private void UpdateCustomSettingsVisibility()
        {
            bool show = this.treeCustomSettings.GetNodeCount(false) > 0;
            this.treeCustomSettings.Visible = show;
            this.btnResetToDefault.Visible = show;
            this.btnCheckAll.Visible = show;
            this.btnUncheckAll.Visible = show;
            this.labelCustomSettings.Visible = show;
        }

        /// <summary>
        /// Update the checked state of all given nodes and their childnodes
        /// based on a dictionary of setting values.
        /// </summary>
        /// 
        private void UpdateNodeCheckedState(Dictionary<string, bool> setting_values, TreeNodeCollection nodes = null)
        {
            if (setting_values == null)
                return;

            UpdateNodeCheckedState(setting => {
                string id = setting.Id;

                if (setting_values.ContainsKey(id))
                    return setting_values[id];

                return setting.Value;
            }, nodes);
        }

        /// <summary>
        /// Generic update on all given nodes and their childnodes, ignoring childnodes for
        /// nodes where the Func returns false.
        /// </summary>
        /// 
        private void UpdateNodesInTree(Func<TreeNode, bool> func, TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                bool include_child_nodes = func(node);
                if (include_child_nodes)
                    UpdateNodesInTree(func, node.Nodes);
            }
        }

        /// <summary>
        /// Update the checked state of all given nodes and their childnodes based on the return
        /// value of the given Func.
        /// </summary>
        /// <param name="nodes">If nodes is null, all nodes of the custom settings tree are affected.</param>
        /// 
        private void UpdateNodeCheckedState(Func<ASLSetting, bool> func, TreeNodeCollection nodes = null)
        {
            if (nodes == null)
                nodes = this.treeCustomSettings.Nodes;

            UpdateNodesInTree(node => {
                var setting = (ASLSetting)node.Tag;
                bool check = func(setting);

                if (node.Checked != check)
                    node.Checked = check;

                return true;
            }, nodes);
        }

        /// <summary>
        /// If the given node is unchecked, grays out all childnodes.
        /// </summary>
        private void UpdateGrayedOut(TreeNode node)
        {
            // Only change color of childnodes if this node isn't already grayed out
            if (node.ForeColor != SystemColors.GrayText)
            {
                UpdateNodesInTree(n => {
                    n.ForeColor = node.Checked ? SystemColors.WindowText : SystemColors.GrayText;
                    return n.Checked;
                }, node.Nodes);
            }
        }


        // Events

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog() {
                FileName = ScriptPath,
                Filter = "Auto Split Script (*.asl)|*.asl|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
                ScriptPath = this.txtScriptPath.Text = dialog.FileName;
        }

        // Basic Setting checked/unchecked
        private void methodCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var checkbox = (CheckBox)sender;
            var setting = (ASLSetting)checkbox.Tag;

            if (setting != null)
            {
                setting.Value = checkbox.Checked;
                _basic_settings_state[setting.Id] = setting.Value;
            }
        }

        // Custom Setting checked/unchecked
        private void settingsTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // Update value in the ASLSetting object, which also changes it in the ASL script
            ASLSetting setting = (ASLSetting)e.Node.Tag;
            setting.Value = e.Node.Checked;
            _custom_settings_state[setting.Id] = setting.Value;

            UpdateGrayedOut(e.Node);
        }


        // Custom Settings Button Events

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            UpdateNodeCheckedState(id => true);
        }

        private void btnUncheckAll_Click(object sender, EventArgs e)
        {
            UpdateNodeCheckedState(id => false);
        }

        private void btnResetToDefault_Click(object sender, EventArgs e)
        {
            UpdateNodeCheckedState(_default_values);
        }


        // Custom Settings Context Menu Events

        private void settingsTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Select clicked node (not only with left-click) for use with context menu
            this.treeCustomSettings.SelectedNode = e.Node;
        }

        private void cmiCheckBranch_Click(object sender, EventArgs e)
        {
            UpdateNodeCheckedState(i => true, this.treeCustomSettings.SelectedNode.Nodes);
        }

        private void cmiUncheckBranch_Click(object sender, EventArgs e)
        {
            UpdateNodeCheckedState(i => false, this.treeCustomSettings.SelectedNode.Nodes);
        }

        private void cmiResetBranchToDefault_Click(object sender, EventArgs e)
        {
            UpdateNodeCheckedState(_default_values, this.treeCustomSettings.SelectedNode.Nodes);
        }

        private void cmiExpandBranch_Click(object sender, EventArgs e)
        {
            this.treeCustomSettings.SelectedNode.ExpandAll();
            this.treeCustomSettings.SelectedNode.EnsureVisible();
        }

        private void cmiCollapseBranch_Click(object sender, EventArgs e)
        {
            this.treeCustomSettings.SelectedNode.Collapse();
            this.treeCustomSettings.SelectedNode.EnsureVisible();
        }

        private void cmiCollapseTreeToSelection_Click(object sender, EventArgs e)
        {
            TreeNode selected = this.treeCustomSettings.SelectedNode;
            this.treeCustomSettings.CollapseAll();
            this.treeCustomSettings.SelectedNode = selected;
            selected.EnsureVisible();
        }

        private void cmiExpandTree_Click(object sender, EventArgs e)
        {
            this.treeCustomSettings.ExpandAll();
            this.treeCustomSettings.SelectedNode.EnsureVisible();
        }

        private void cmiCollapseTree_Click(object sender, EventArgs e)
        {
            this.treeCustomSettings.CollapseAll();
        }

        private void cmiResetSettingToDefault_Click(object sender, EventArgs e)
        {
            TreeNode node = this.treeCustomSettings.SelectedNode;
            ASLSetting setting = (ASLSetting)node.Tag;
            if (_default_values != null && _default_values.ContainsKey(setting.Id))
                node.Checked = _default_values[setting.Id];
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
                var local_pos = PointToClient(Cursor.Position);
                var hit_test_info = HitTest(local_pos);

                if (hit_test_info.Location == TreeViewHitTestLocations.StateImage)
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
