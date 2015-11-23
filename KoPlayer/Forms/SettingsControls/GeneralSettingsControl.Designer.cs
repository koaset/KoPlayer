namespace KoPlayer.Forms.SettingsControls
{
    partial class GeneralSettingsControl
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
            this.startup_checkbox = new System.Windows.Forms.CheckBox();
            this.tray_checkbox = new System.Windows.Forms.CheckBox();
            this.popup_checkbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // title_label
            // 
            this.title_label.AutoSize = true;
            this.title_label.Location = new System.Drawing.Point(14, 2);
            this.title_label.Name = "title_label";
            this.title_label.Size = new System.Drawing.Size(85, 13);
            this.title_label.TabIndex = 14;
            this.title_label.Text = "General Settings";
            // 
            // startup_checkbox
            // 
            this.startup_checkbox.AutoSize = true;
            this.startup_checkbox.Location = new System.Drawing.Point(17, 52);
            this.startup_checkbox.Name = "startup_checkbox";
            this.startup_checkbox.Size = new System.Drawing.Size(93, 17);
            this.startup_checkbox.TabIndex = 15;
            this.startup_checkbox.Text = "Run at startup";
            this.startup_checkbox.UseVisualStyleBackColor = true;
            this.startup_checkbox.CheckedChanged += new System.EventHandler(this.checkbox_CheckedChanged);
            // 
            // tray_checkbox
            // 
            this.tray_checkbox.AutoSize = true;
            this.tray_checkbox.Location = new System.Drawing.Point(17, 92);
            this.tray_checkbox.Name = "tray_checkbox";
            this.tray_checkbox.Size = new System.Drawing.Size(133, 17);
            this.tray_checkbox.TabIndex = 16;
            this.tray_checkbox.Text = "Minimize to system tray";
            this.tray_checkbox.UseVisualStyleBackColor = true;
            this.tray_checkbox.CheckedChanged += new System.EventHandler(this.checkbox_CheckedChanged);
            // 
            // popup_checkbox
            // 
            this.popup_checkbox.AutoSize = true;
            this.popup_checkbox.Location = new System.Drawing.Point(17, 132);
            this.popup_checkbox.Name = "popup_checkbox";
            this.popup_checkbox.Size = new System.Drawing.Size(213, 17);
            this.popup_checkbox.TabIndex = 17;
            this.popup_checkbox.Text = "Show song info popup on track change";
            this.popup_checkbox.UseVisualStyleBackColor = true;
            this.popup_checkbox.CheckedChanged += new System.EventHandler(this.checkbox_CheckedChanged);
            // 
            // GeneralSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.popup_checkbox);
            this.Controls.Add(this.tray_checkbox);
            this.Controls.Add(this.startup_checkbox);
            this.Controls.Add(this.title_label);
            this.Name = "GeneralSettingsControl";
            this.Size = new System.Drawing.Size(231, 192);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title_label;
        private System.Windows.Forms.CheckBox startup_checkbox;
        private System.Windows.Forms.CheckBox tray_checkbox;
        private System.Windows.Forms.CheckBox popup_checkbox;
    }
}
