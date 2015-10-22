namespace KoPlayer.Forms.SettingsControls
{
    partial class GlobalHotkeySettingsControl
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
            this.command_list = new System.Windows.Forms.ListView();
            this.actioncolumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.keycolumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.setkey_button = new System.Windows.Forms.Button();
            this.command_box = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // title_label
            // 
            this.title_label.AutoSize = true;
            this.title_label.Location = new System.Drawing.Point(14, 2);
            this.title_label.Name = "title_label";
            this.title_label.Size = new System.Drawing.Size(82, 13);
            this.title_label.TabIndex = 13;
            this.title_label.Text = "Hotkey Settings";
            // 
            // command_list
            // 
            this.command_list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.actioncolumn,
            this.keycolumn});
            this.command_list.FullRowSelect = true;
            this.command_list.GridLines = true;
            this.command_list.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.command_list.Location = new System.Drawing.Point(0, 22);
            this.command_list.MultiSelect = false;
            this.command_list.Name = "command_list";
            this.command_list.Size = new System.Drawing.Size(228, 130);
            this.command_list.TabIndex = 14;
            this.command_list.UseCompatibleStateImageBehavior = false;
            this.command_list.View = System.Windows.Forms.View.Details;
            // 
            // actioncolumn
            // 
            this.actioncolumn.Text = "Action";
            this.actioncolumn.Width = 123;
            // 
            // keycolumn
            // 
            this.keycolumn.Text = "Keys";
            this.keycolumn.Width = 123;
            // 
            // setkey_button
            // 
            this.setkey_button.Location = new System.Drawing.Point(5, 158);
            this.setkey_button.Name = "setkey_button";
            this.setkey_button.Size = new System.Drawing.Size(75, 23);
            this.setkey_button.TabIndex = 15;
            this.setkey_button.Text = "Set Key";
            this.setkey_button.UseVisualStyleBackColor = true;
            this.setkey_button.Click += new System.EventHandler(this.setkey_button_Click);
            // 
            // command_box
            // 
            this.command_box.Location = new System.Drawing.Point(93, 161);
            this.command_box.Name = "command_box";
            this.command_box.Size = new System.Drawing.Size(100, 20);
            this.command_box.TabIndex = 16;
            this.command_box.KeyDown += new System.Windows.Forms.KeyEventHandler(this.command_box_KeyDown);
            // 
            // GlobalHotkeySettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.command_box);
            this.Controls.Add(this.setkey_button);
            this.Controls.Add(this.command_list);
            this.Controls.Add(this.title_label);
            this.Name = "GlobalHotkeySettingsControl";
            this.Size = new System.Drawing.Size(231, 192);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title_label;
        private System.Windows.Forms.ListView command_list;
        private System.Windows.Forms.Button setkey_button;
        private System.Windows.Forms.TextBox command_box;
        private System.Windows.Forms.ColumnHeader actioncolumn;
        private System.Windows.Forms.ColumnHeader keycolumn;

    }
}
