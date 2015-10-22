namespace KoPlayer.Forms.SettingsControls
{
    partial class LastFMSettingsControl
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
            this.password_label = new System.Windows.Forms.Label();
            this.username_label = new System.Windows.Forms.Label();
            this.enable_checkbox = new System.Windows.Forms.CheckBox();
            this.connect_button = new System.Windows.Forms.Button();
            this.username_box = new System.Windows.Forms.TextBox();
            this.password_box = new System.Windows.Forms.TextBox();
            this.status_label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // title_label
            // 
            this.title_label.AutoSize = true;
            this.title_label.Location = new System.Drawing.Point(13, 12);
            this.title_label.Name = "title_label";
            this.title_label.Size = new System.Drawing.Size(133, 13);
            this.title_label.TabIndex = 13;
            this.title_label.Text = "Last.fm  Scrobbler Settings";
            // 
            // password_label
            // 
            this.password_label.AutoSize = true;
            this.password_label.Location = new System.Drawing.Point(39, 104);
            this.password_label.Name = "password_label";
            this.password_label.Size = new System.Drawing.Size(53, 13);
            this.password_label.TabIndex = 12;
            this.password_label.Text = "Password";
            // 
            // username_label
            // 
            this.username_label.AutoSize = true;
            this.username_label.Location = new System.Drawing.Point(39, 65);
            this.username_label.Name = "username_label";
            this.username_label.Size = new System.Drawing.Size(55, 13);
            this.username_label.TabIndex = 10;
            this.username_label.Text = "Username";
            // 
            // enable_checkbox
            // 
            this.enable_checkbox.AutoSize = true;
            this.enable_checkbox.Location = new System.Drawing.Point(16, 41);
            this.enable_checkbox.Name = "enable_checkbox";
            this.enable_checkbox.Size = new System.Drawing.Size(59, 17);
            this.enable_checkbox.TabIndex = 14;
            this.enable_checkbox.Text = "Enable";
            this.enable_checkbox.UseVisualStyleBackColor = true;
            // 
            // connect_button
            // 
            this.connect_button.Location = new System.Drawing.Point(16, 150);
            this.connect_button.Name = "connect_button";
            this.connect_button.Size = new System.Drawing.Size(75, 23);
            this.connect_button.TabIndex = 15;
            this.connect_button.Text = "Connect";
            this.connect_button.UseVisualStyleBackColor = true;
            // 
            // username_box
            // 
            this.username_box.Location = new System.Drawing.Point(42, 81);
            this.username_box.Name = "username_box";
            this.username_box.Size = new System.Drawing.Size(112, 20);
            this.username_box.TabIndex = 16;
            // 
            // password_box
            // 
            this.password_box.Location = new System.Drawing.Point(42, 119);
            this.password_box.Name = "password_box";
            this.password_box.PasswordChar = '*';
            this.password_box.Size = new System.Drawing.Size(112, 20);
            this.password_box.TabIndex = 17;
            // 
            // status_label
            // 
            this.status_label.AutoSize = true;
            this.status_label.Location = new System.Drawing.Point(106, 155);
            this.status_label.Name = "status_label";
            this.status_label.Size = new System.Drawing.Size(78, 13);
            this.status_label.TabIndex = 18;
            this.status_label.Text = "Not connected";
            // 
            // LastFMSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.status_label);
            this.Controls.Add(this.password_box);
            this.Controls.Add(this.username_box);
            this.Controls.Add(this.connect_button);
            this.Controls.Add(this.enable_checkbox);
            this.Controls.Add(this.title_label);
            this.Controls.Add(this.password_label);
            this.Controls.Add(this.username_label);
            this.Name = "LastFMSettingsControl";
            this.Size = new System.Drawing.Size(205, 186);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title_label;
        private System.Windows.Forms.Label password_label;
        private System.Windows.Forms.Label username_label;
        private System.Windows.Forms.CheckBox enable_checkbox;
        private System.Windows.Forms.Button connect_button;
        private System.Windows.Forms.TextBox username_box;
        private System.Windows.Forms.TextBox password_box;
        private System.Windows.Forms.Label status_label;

    }
}
