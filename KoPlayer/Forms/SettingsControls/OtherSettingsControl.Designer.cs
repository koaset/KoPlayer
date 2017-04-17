namespace KoPlayer.Forms.SettingsControls
{
    partial class OtherSettingsControl
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
            this.title_label = new System.Windows.Forms.Label();
            this.formatLabel = new System.Windows.Forms.Label();
            this.nowPlayingEnabledcheckBox = new System.Windows.Forms.CheckBox();
            this.formatTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // title_label
            // 
            this.title_label.AutoSize = true;
            this.title_label.Location = new System.Drawing.Point(14, 2);
            this.title_label.Name = "title_label";
            this.title_label.Size = new System.Drawing.Size(74, 13);
            this.title_label.TabIndex = 14;
            this.title_label.Text = "Other Settings";
            // 
            // formatLabel
            // 
            this.formatLabel.AutoSize = true;
            this.formatLabel.Location = new System.Drawing.Point(14, 86);
            this.formatLabel.Name = "formatLabel";
            this.formatLabel.Size = new System.Drawing.Size(39, 13);
            this.formatLabel.TabIndex = 16;
            this.formatLabel.Text = "Format";
            // 
            // nowPlayingEnabledcheckBox
            // 
            this.nowPlayingEnabledcheckBox.AutoSize = true;
            this.nowPlayingEnabledcheckBox.Location = new System.Drawing.Point(17, 55);
            this.nowPlayingEnabledcheckBox.Name = "nowPlayingEnabledcheckBox";
            this.nowPlayingEnabledcheckBox.Size = new System.Drawing.Size(138, 17);
            this.nowPlayingEnabledcheckBox.TabIndex = 17;
            this.nowPlayingEnabledcheckBox.Text = "Save now playing to file";
            this.nowPlayingEnabledcheckBox.UseVisualStyleBackColor = true;
            this.nowPlayingEnabledcheckBox.CheckedChanged += new System.EventHandler(this.nowPlayingEnabledcheckBox_CheckedChanged);
            // 
            // formatTextBox
            // 
            this.formatTextBox.Location = new System.Drawing.Point(17, 102);
            this.formatTextBox.Name = "formatTextBox";
            this.formatTextBox.Size = new System.Drawing.Size(197, 20);
            this.formatTextBox.TabIndex = 18;
            this.formatTextBox.TextChanged += new System.EventHandler(this.formatTextBox_TextChanged);
            // 
            // OtherSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.formatTextBox);
            this.Controls.Add(this.nowPlayingEnabledcheckBox);
            this.Controls.Add(this.formatLabel);
            this.Controls.Add(this.title_label);
            this.Name = "OtherSettingsControl";
            this.Size = new System.Drawing.Size(231, 192);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title_label;
        private System.Windows.Forms.Label formatLabel;
        private System.Windows.Forms.CheckBox nowPlayingEnabledcheckBox;
        private System.Windows.Forms.TextBox formatTextBox;
    }
}
