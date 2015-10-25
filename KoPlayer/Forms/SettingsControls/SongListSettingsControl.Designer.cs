namespace KoPlayer.Forms.SettingsControls
{
    partial class SongListSettingsControl
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
            this.fontsize_label = new System.Windows.Forms.Label();
            this.fontsize_box = new System.Windows.Forms.NumericUpDown();
            this.rowheight_label = new System.Windows.Forms.Label();
            this.rowheight_box = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.fontsize_box)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rowheight_box)).BeginInit();
            this.SuspendLayout();
            // 
            // title_label
            // 
            this.title_label.AutoSize = true;
            this.title_label.Location = new System.Drawing.Point(14, 2);
            this.title_label.Name = "title_label";
            this.title_label.Size = new System.Drawing.Size(92, 13);
            this.title_label.TabIndex = 13;
            this.title_label.Text = "Song List Settings";
            // 
            // fontsize_label
            // 
            this.fontsize_label.AutoSize = true;
            this.fontsize_label.Location = new System.Drawing.Point(13, 100);
            this.fontsize_label.Name = "fontsize_label";
            this.fontsize_label.Size = new System.Drawing.Size(51, 13);
            this.fontsize_label.TabIndex = 12;
            this.fontsize_label.Text = "Font Size";
            // 
            // fontsize_box
            // 
            this.fontsize_box.DecimalPlaces = 2;
            this.fontsize_box.Location = new System.Drawing.Point(16, 116);
            this.fontsize_box.Name = "fontsize_box";
            this.fontsize_box.Size = new System.Drawing.Size(120, 20);
            this.fontsize_box.TabIndex = 11;
            this.fontsize_box.Leave += new System.EventHandler(this.fontsize_box_Leave);
            // 
            // rowheight_label
            // 
            this.rowheight_label.AutoSize = true;
            this.rowheight_label.Location = new System.Drawing.Point(13, 50);
            this.rowheight_label.Name = "rowheight_label";
            this.rowheight_label.Size = new System.Drawing.Size(63, 13);
            this.rowheight_label.TabIndex = 10;
            this.rowheight_label.Text = "Row Height";
            // 
            // rowheight_box
            // 
            this.rowheight_box.Location = new System.Drawing.Point(16, 66);
            this.rowheight_box.Name = "rowheight_box";
            this.rowheight_box.Size = new System.Drawing.Size(120, 20);
            this.rowheight_box.TabIndex = 9;
            this.rowheight_box.Leave += new System.EventHandler(this.rowheight_box_Leave);
            // 
            // SongListSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.title_label);
            this.Controls.Add(this.fontsize_label);
            this.Controls.Add(this.fontsize_box);
            this.Controls.Add(this.rowheight_label);
            this.Controls.Add(this.rowheight_box);
            this.Name = "SongListSettingsControl";
            this.Size = new System.Drawing.Size(205, 186);
            ((System.ComponentModel.ISupportInitialize)(this.fontsize_box)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rowheight_box)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title_label;
        private System.Windows.Forms.Label fontsize_label;
        private System.Windows.Forms.NumericUpDown fontsize_box;
        private System.Windows.Forms.Label rowheight_label;
        private System.Windows.Forms.NumericUpDown rowheight_box;

    }
}
