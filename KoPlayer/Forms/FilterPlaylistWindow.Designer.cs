namespace KoPlayer.Forms
{
    partial class FilterPlaylistWindow
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
            this.create_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.filter_listbox = new System.Windows.Forms.ListBox();
            this.addFilter_button = new System.Windows.Forms.Button();
            this.removeFilter_button = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.filter_label = new System.Windows.Forms.Label();
            this.editFilter_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // create_button
            // 
            this.create_button.Location = new System.Drawing.Point(154, 134);
            this.create_button.Name = "create_button";
            this.create_button.Size = new System.Drawing.Size(75, 23);
            this.create_button.TabIndex = 0;
            this.create_button.Text = "Create";
            this.create_button.UseVisualStyleBackColor = true;
            this.create_button.Click += new System.EventHandler(this.create_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(235, 134);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 1;
            this.cancel_button.Text = "Cancel";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // filter_listbox
            // 
            this.filter_listbox.FormattingEnabled = true;
            this.filter_listbox.Location = new System.Drawing.Point(9, 30);
            this.filter_listbox.Name = "filter_listbox";
            this.filter_listbox.Size = new System.Drawing.Size(301, 69);
            this.filter_listbox.TabIndex = 8;
            // 
            // addFilter_button
            // 
            this.addFilter_button.Location = new System.Drawing.Point(9, 105);
            this.addFilter_button.Name = "addFilter_button";
            this.addFilter_button.Size = new System.Drawing.Size(23, 23);
            this.addFilter_button.TabIndex = 9;
            this.addFilter_button.Text = "+";
            this.addFilter_button.UseVisualStyleBackColor = true;
            this.addFilter_button.Click += new System.EventHandler(this.addFilter_button_Click);
            // 
            // removeFilter_button
            // 
            this.removeFilter_button.Location = new System.Drawing.Point(38, 105);
            this.removeFilter_button.Name = "removeFilter_button";
            this.removeFilter_button.Size = new System.Drawing.Size(23, 23);
            this.removeFilter_button.TabIndex = 10;
            this.removeFilter_button.Text = "-";
            this.removeFilter_button.UseVisualStyleBackColor = true;
            this.removeFilter_button.Click += new System.EventHandler(this.removeFilter_button_Click);
            // 
            // filter_label
            // 
            this.filter_label.AutoSize = true;
            this.filter_label.Location = new System.Drawing.Point(6, 14);
            this.filter_label.Name = "filter_label";
            this.filter_label.Size = new System.Drawing.Size(34, 13);
            this.filter_label.TabIndex = 11;
            this.filter_label.Text = "Filters";
            // 
            // editFilter_button
            // 
            this.editFilter_button.Location = new System.Drawing.Point(67, 105);
            this.editFilter_button.Name = "editFilter_button";
            this.editFilter_button.Size = new System.Drawing.Size(56, 23);
            this.editFilter_button.TabIndex = 12;
            this.editFilter_button.Text = "Edit";
            this.editFilter_button.UseVisualStyleBackColor = true;
            this.editFilter_button.Click += new System.EventHandler(this.editFilter_button_Click);
            // 
            // FilterPlaylistWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(321, 169);
            this.Controls.Add(this.editFilter_button);
            this.Controls.Add(this.filter_label);
            this.Controls.Add(this.removeFilter_button);
            this.Controls.Add(this.addFilter_button);
            this.Controls.Add(this.filter_listbox);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.create_button);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FilterPlaylistWindow";
            this.ShowInTaskbar = false;
            this.Text = "Rating Filter Playlist";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button create_button;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.ListBox filter_listbox;
        private System.Windows.Forms.Button addFilter_button;
        private System.Windows.Forms.Button removeFilter_button;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label filter_label;
        private System.Windows.Forms.Button editFilter_button;
    }
}