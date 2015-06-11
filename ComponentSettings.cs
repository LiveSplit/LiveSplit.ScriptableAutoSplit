using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFormsColor;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class ComponentSettings : UserControl
    {
        public String ScriptPath { get; set; }

        public ComponentSettings()
        {
            InitializeComponent();

            ScriptPath = "";

            txtScriptPath.DataBindings.Add("Text", this, "ScriptPath", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "Version", "1.4"));
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "ScriptPath", ScriptPath));
            return settingsNode;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            var element = (XmlElement)settings;
            if (!element.IsEmpty)
            {
                ScriptPath = SettingsHelper.ParseString(element["ScriptPath"], string.Empty);
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
    }
}
