namespace KoPlayer.Forms.SettingsControls
{
    partial class AudioSettingsControl
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
            this.devices_comboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // title_label
            // 
            this.title_label.AutoSize = true;
            this.title_label.Location = new System.Drawing.Point(14, 2);
            this.title_label.Name = "title_label";
            this.title_label.Size = new System.Drawing.Size(75, 13);
            this.title_label.TabIndex = 14;
            this.title_label.Text = "Audio Settings";
            // 
            // devices_comboBox
            // 
            this.devices_comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.devices_comboBox.FormattingEnabled = true;
            this.devices_comboBox.Location = new System.Drawing.Point(17, 79);
            this.devices_comboBox.Name = "devices_comboBox";
            this.devices_comboBox.Size = new System.Drawing.Size(201, 21);
            this.devices_comboBox.TabIndex = 15;
            this.devices_comboBox.SelectedIndexChanged += new System.EventHandler(this.devices_comboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Playback Device";
            // 
            // AudioSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.devices_comboBox);
            this.Controls.Add(this.title_label);
            this.Name = "AudioSettingsControl";
            this.Size = new System.Drawing.Size(231, 192);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title_label;
        private System.Windows.Forms.ComboBox devices_comboBox;
        private System.Windows.Forms.Label label1;
    }
}
