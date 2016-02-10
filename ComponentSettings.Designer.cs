namespace LiveSplit.UI.Components
{
    partial class ComponentSettings
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.txtScriptPath = new System.Windows.Forms.TextBox();
            this.customSettingsList = new System.Windows.Forms.CheckedListBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.checkAllButton = new System.Windows.Forms.Button();
            this.uncheckAllButton = new System.Windows.Forms.Button();
            this.resetToDefaultButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.startCheckbox = new System.Windows.Forms.CheckBox();
            this.splitCheckbox = new System.Windows.Forms.CheckBox();
            this.resetCheckbox = new System.Windows.Forms.CheckBox();
            this.gameVersionLabel = new System.Windows.Forms.Label();
            this.optionsLabel = new System.Windows.Forms.Label();
            this.customSettingsLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 76F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSelectFile, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtScriptPath, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.customSettingsList, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.optionsLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.customSettingsLabel, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 7);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(462, 498);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Script Path:";
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectFile.Location = new System.Drawing.Point(385, 3);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(74, 23);
            this.btnSelectFile.TabIndex = 1;
            this.btnSelectFile.Text = "Browse...";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // txtScriptPath
            // 
            this.txtScriptPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtScriptPath.Location = new System.Drawing.Point(79, 4);
            this.txtScriptPath.Name = "txtScriptPath";
            this.txtScriptPath.Size = new System.Drawing.Size(300, 20);
            this.txtScriptPath.TabIndex = 0;
            // 
            // customSettingsList
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.customSettingsList, 2);
            this.customSettingsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customSettingsList.FormattingEnabled = true;
            this.customSettingsList.HorizontalScrollbar = true;
            this.customSettingsList.Location = new System.Drawing.Point(79, 61);
            this.customSettingsList.Name = "customSettingsList";
            this.customSettingsList.Size = new System.Drawing.Size(380, 399);
            this.customSettingsList.TabIndex = 4;
            this.customSettingsList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.customSettingsList_ItemCheck);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.checkAllButton);
            this.flowLayoutPanel1.Controls.Add(this.uncheckAllButton);
            this.flowLayoutPanel1.Controls.Add(this.resetToDefaultButton);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(206, 466);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(253, 29);
            this.flowLayoutPanel1.TabIndex = 8;
            // 
            // checkAllButton
            // 
            this.checkAllButton.Location = new System.Drawing.Point(3, 3);
            this.checkAllButton.Name = "checkAllButton";
            this.checkAllButton.Size = new System.Drawing.Size(62, 23);
            this.checkAllButton.TabIndex = 5;
            this.checkAllButton.Text = "Check all";
            this.checkAllButton.UseVisualStyleBackColor = true;
            this.checkAllButton.Click += new System.EventHandler(this.checkAllButton_Click);
            // 
            // uncheckAllButton
            // 
            this.uncheckAllButton.AutoSize = true;
            this.uncheckAllButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.uncheckAllButton.Location = new System.Drawing.Point(71, 3);
            this.uncheckAllButton.Name = "uncheckAllButton";
            this.uncheckAllButton.Size = new System.Drawing.Size(74, 23);
            this.uncheckAllButton.TabIndex = 6;
            this.uncheckAllButton.Text = "Uncheck all";
            this.uncheckAllButton.UseVisualStyleBackColor = true;
            this.uncheckAllButton.Click += new System.EventHandler(this.uncheckAllButton_Click);
            // 
            // resetToDefaultButton
            // 
            this.resetToDefaultButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.resetToDefaultButton.Location = new System.Drawing.Point(151, 3);
            this.resetToDefaultButton.Name = "resetToDefaultButton";
            this.resetToDefaultButton.Size = new System.Drawing.Size(99, 23);
            this.resetToDefaultButton.TabIndex = 7;
            this.resetToDefaultButton.Text = "Reset to default";
            this.resetToDefaultButton.UseVisualStyleBackColor = true;
            this.resetToDefaultButton.Click += new System.EventHandler(this.resetToDefaultButton_Click);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel2, 2);
            this.flowLayoutPanel2.Controls.Add(this.startCheckbox);
            this.flowLayoutPanel2.Controls.Add(this.splitCheckbox);
            this.flowLayoutPanel2.Controls.Add(this.resetCheckbox);
            this.flowLayoutPanel2.Controls.Add(this.gameVersionLabel);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(79, 32);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(380, 23);
            this.flowLayoutPanel2.TabIndex = 12;
            // 
            // startCheckbox
            // 
            this.startCheckbox.Checked = true;
            this.startCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.startCheckbox.Location = new System.Drawing.Point(3, 3);
            this.startCheckbox.Name = "startCheckbox";
            this.startCheckbox.Size = new System.Drawing.Size(48, 17);
            this.startCheckbox.TabIndex = 11;
            this.startCheckbox.Text = "Start";
            this.startCheckbox.UseVisualStyleBackColor = true;
            this.startCheckbox.CheckedChanged += new System.EventHandler(this.methodCheckbox_CheckedChanged);
            // 
            // splitCheckbox
            // 
            this.splitCheckbox.Checked = true;
            this.splitCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.splitCheckbox.Location = new System.Drawing.Point(57, 3);
            this.splitCheckbox.Name = "splitCheckbox";
            this.splitCheckbox.Size = new System.Drawing.Size(46, 17);
            this.splitCheckbox.TabIndex = 0;
            this.splitCheckbox.Text = "Split";
            this.splitCheckbox.UseVisualStyleBackColor = true;
            this.splitCheckbox.CheckedChanged += new System.EventHandler(this.methodCheckbox_CheckedChanged);
            // 
            // resetCheckbox
            // 
            this.resetCheckbox.Checked = true;
            this.resetCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.resetCheckbox.Location = new System.Drawing.Point(109, 3);
            this.resetCheckbox.Name = "resetCheckbox";
            this.resetCheckbox.Size = new System.Drawing.Size(54, 17);
            this.resetCheckbox.TabIndex = 0;
            this.resetCheckbox.Text = "Reset";
            this.resetCheckbox.UseVisualStyleBackColor = true;
            this.resetCheckbox.CheckedChanged += new System.EventHandler(this.methodCheckbox_CheckedChanged);
            // 
            // gameVersionLabel
            // 
            this.gameVersionLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.gameVersionLabel.AutoEllipsis = true;
            this.gameVersionLabel.Location = new System.Drawing.Point(169, 5);
            this.gameVersionLabel.Name = "gameVersionLabel";
            this.gameVersionLabel.Size = new System.Drawing.Size(208, 13);
            this.gameVersionLabel.TabIndex = 10;
            this.gameVersionLabel.Text = "Game Version: 1.0";
            this.gameVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // optionsLabel
            // 
            this.optionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.optionsLabel.AutoSize = true;
            this.optionsLabel.Location = new System.Drawing.Point(3, 37);
            this.optionsLabel.Name = "optionsLabel";
            this.optionsLabel.Size = new System.Drawing.Size(70, 13);
            this.optionsLabel.TabIndex = 9;
            this.optionsLabel.Text = "Options:";
            // 
            // customSettingsLabel
            // 
            this.customSettingsLabel.AutoSize = true;
            this.customSettingsLabel.Location = new System.Drawing.Point(3, 63);
            this.customSettingsLabel.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.customSettingsLabel.Name = "customSettingsLabel";
            this.customSettingsLabel.Size = new System.Drawing.Size(59, 13);
            this.customSettingsLabel.TabIndex = 13;
            this.customSettingsLabel.Text = "Advanced:";
            // 
            // ComponentSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ComponentSettings";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.Size = new System.Drawing.Size(476, 512);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelectFile;
        public System.Windows.Forms.TextBox txtScriptPath;
        private System.Windows.Forms.CheckedListBox customSettingsList;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button checkAllButton;
        private System.Windows.Forms.Button uncheckAllButton;
        private System.Windows.Forms.Button resetToDefaultButton;
        private System.Windows.Forms.Label optionsLabel;
        private System.Windows.Forms.Label gameVersionLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.CheckBox startCheckbox;
        private System.Windows.Forms.CheckBox resetCheckbox;
        private System.Windows.Forms.CheckBox splitCheckbox;
        private System.Windows.Forms.Label customSettingsLabel;
    }
}
