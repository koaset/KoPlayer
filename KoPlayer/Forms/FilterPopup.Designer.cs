namespace KoPlayer.Forms
{
    partial class FilterPopup
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
            this.ok_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.filterType_combobox = new System.Windows.Forms.ComboBox();
            this.searchString_textbox = new System.Windows.Forms.TextBox();
            this.filterParams_combobox = new System.Windows.Forms.ComboBox();
            this.ratingBox1 = new KoPlayer.Forms.RatingBox();
            this.date_box = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.ratingBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.date_box)).BeginInit();
            this.SuspendLayout();
            // 
            // ok_button
            // 
            this.ok_button.Location = new System.Drawing.Point(213, 48);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(75, 23);
            this.ok_button.TabIndex = 0;
            this.ok_button.Text = "Add";
            this.ok_button.UseVisualStyleBackColor = true;
            this.ok_button.Click += new System.EventHandler(this.ok_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(294, 48);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 1;
            this.cancel_button.Text = "Cancel";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // filterType_combobox
            // 
            this.filterType_combobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.filterType_combobox.FormattingEnabled = true;
            this.filterType_combobox.Location = new System.Drawing.Point(12, 12);
            this.filterType_combobox.Name = "filterType_combobox";
            this.filterType_combobox.Size = new System.Drawing.Size(91, 21);
            this.filterType_combobox.TabIndex = 2;
            this.filterType_combobox.SelectionChangeCommitted += new System.EventHandler(this.filterType_combobox_SelectionChangeCommitted);
            // 
            // searchString_textbox
            // 
            this.searchString_textbox.Location = new System.Drawing.Point(261, 13);
            this.searchString_textbox.Name = "searchString_textbox";
            this.searchString_textbox.Size = new System.Drawing.Size(108, 20);
            this.searchString_textbox.TabIndex = 4;
            // 
            // filterParams_combobox
            // 
            this.filterParams_combobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.filterParams_combobox.FormattingEnabled = true;
            this.filterParams_combobox.Location = new System.Drawing.Point(119, 12);
            this.filterParams_combobox.Name = "filterParams_combobox";
            this.filterParams_combobox.Size = new System.Drawing.Size(126, 21);
            this.filterParams_combobox.TabIndex = 6;
            // 
            // ratingBox1
            // 
            this.ratingBox1.Location = new System.Drawing.Point(261, 14);
            this.ratingBox1.Name = "ratingBox1";
            this.ratingBox1.Size = new System.Drawing.Size(108, 20);
            this.ratingBox1.TabIndex = 7;
            // 
            // date_box
            // 
            this.date_box.Location = new System.Drawing.Point(263, 14);
            this.date_box.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.date_box.Name = "date_box";
            this.date_box.Size = new System.Drawing.Size(106, 20);
            this.date_box.TabIndex = 8;
            this.date_box.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // FilterPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 81);
            this.Controls.Add(this.date_box);
            this.Controls.Add(this.ratingBox1);
            this.Controls.Add(this.filterParams_combobox);
            this.Controls.Add(this.searchString_textbox);
            this.Controls.Add(this.filterType_combobox);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.ok_button);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilterPopup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "New filter";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.ratingBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.date_box)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ok_button;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.ComboBox filterType_combobox;
        private System.Windows.Forms.TextBox searchString_textbox;
        private System.Windows.Forms.ComboBox filterParams_combobox;
        private RatingBox ratingBox1;
        private System.Windows.Forms.NumericUpDown date_box;
    }
}