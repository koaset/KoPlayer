namespace KoPlayer.Forms
{
    partial class SettingsWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.save_Button = new System.Windows.Forms.Button();
            this.cancel_Button = new System.Windows.Forms.Button();
            this.settingsCategoryView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // save_Button
            // 
            this.save_Button.Location = new System.Drawing.Point(235, 205);
            this.save_Button.Name = "save_Button";
            this.save_Button.Size = new System.Drawing.Size(75, 23);
            this.save_Button.TabIndex = 1;
            this.save_Button.Text = "Save";
            this.save_Button.UseVisualStyleBackColor = true;
            this.save_Button.Click += new System.EventHandler(this.save_Button_Click);
            // 
            // cancel_Button
            // 
            this.cancel_Button.Location = new System.Drawing.Point(316, 205);
            this.cancel_Button.Name = "cancel_Button";
            this.cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.cancel_Button.TabIndex = 2;
            this.cancel_Button.Text = "Cancel";
            this.cancel_Button.UseVisualStyleBackColor = true;
            this.cancel_Button.Click += new System.EventHandler(this.cancel_Button_Click);
            // 
            // settingsCategoryView
            // 
            this.settingsCategoryView.Location = new System.Drawing.Point(12, 12);
            this.settingsCategoryView.Name = "settingsCategoryView";
            this.settingsCategoryView.Size = new System.Drawing.Size(171, 216);
            this.settingsCategoryView.TabIndex = 3;
            this.settingsCategoryView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.settingsCategoryView_AfterSelect);
            // 
            // SettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 240);
            this.Controls.Add(this.settingsCategoryView);
            this.Controls.Add(this.cancel_Button);
            this.Controls.Add(this.save_Button);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsWindow";
            this.ShowInTaskbar = false;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.PreferencesWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button save_Button;
        private System.Windows.Forms.Button cancel_Button;
        private System.Windows.Forms.TreeView settingsCategoryView;
    }
}