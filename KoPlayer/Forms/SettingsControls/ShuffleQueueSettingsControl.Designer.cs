namespace KoPlayer.Forms.SettingsControls
{
    partial class ShuffleQueueSettingsControl
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
            this.nextsongs_label = new System.Windows.Forms.Label();
            this.upcoming_box = new System.Windows.Forms.NumericUpDown();
            this.prevsongs_label = new System.Windows.Forms.Label();
            this.recent_box = new System.Windows.Forms.NumericUpDown();
            this.source_label = new System.Windows.Forms.Label();
            this.playlist_box = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.upcoming_box)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.recent_box)).BeginInit();
            this.SuspendLayout();
            // 
            // title_label
            // 
            this.title_label.AutoSize = true;
            this.title_label.Location = new System.Drawing.Point(13, 12);
            this.title_label.Name = "title_label";
            this.title_label.Size = new System.Drawing.Size(91, 13);
            this.title_label.TabIndex = 13;
            this.title_label.Text = "Shuffle Queue Settings";
            // 
            // nextsongs_label
            // 
            this.nextsongs_label.AutoSize = true;
            this.nextsongs_label.Location = new System.Drawing.Point(13, 127);
            this.nextsongs_label.Name = "nextsongs_label";
            this.nextsongs_label.Size = new System.Drawing.Size(119, 13);
            this.nextsongs_label.TabIndex = 12;
            this.nextsongs_label.Text = "Upcoming song number";
            // 
            // upcoming_box
            // 
            this.upcoming_box.Location = new System.Drawing.Point(16, 143);
            this.upcoming_box.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.upcoming_box.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.upcoming_box.Name = "upcoming_box";
            this.upcoming_box.Size = new System.Drawing.Size(120, 20);
            this.upcoming_box.TabIndex = 11;
            this.upcoming_box.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.upcoming_box.ValueChanged += new System.EventHandler(this.upcoming_box_ValueChanged);
            // 
            // prevsongs_label
            // 
            this.prevsongs_label.AutoSize = true;
            this.prevsongs_label.Location = new System.Drawing.Point(13, 83);
            this.prevsongs_label.Name = "prevsongs_label";
            this.prevsongs_label.Size = new System.Drawing.Size(106, 13);
            this.prevsongs_label.TabIndex = 10;
            this.prevsongs_label.Text = "Recent song number";
            // 
            // recent_box
            // 
            this.recent_box.Location = new System.Drawing.Point(16, 99);
            this.recent_box.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.recent_box.Name = "recent_box";
            this.recent_box.Size = new System.Drawing.Size(120, 20);
            this.recent_box.TabIndex = 9;
            this.recent_box.ValueChanged += new System.EventHandler(this.recent_box_ValueChanged);
            // 
            // source_label
            // 
            this.source_label.AutoSize = true;
            this.source_label.Location = new System.Drawing.Point(13, 38);
            this.source_label.Name = "source_label";
            this.source_label.Size = new System.Drawing.Size(76, 13);
            this.source_label.TabIndex = 15;
            this.source_label.Text = "Source Playlist";
            // 
            // playlist_box
            // 
            this.playlist_box.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.playlist_box.FormattingEnabled = true;
            this.playlist_box.Location = new System.Drawing.Point(16, 55);
            this.playlist_box.Name = "playlist_box";
            this.playlist_box.Size = new System.Drawing.Size(121, 21);
            this.playlist_box.TabIndex = 16;
            this.playlist_box.SelectedIndexChanged += new System.EventHandler(this.playlist_box_SelectedIndexChanged);
            // 
            // ShuffleQueueSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.playlist_box);
            this.Controls.Add(this.source_label);
            this.Controls.Add(this.title_label);
            this.Controls.Add(this.nextsongs_label);
            this.Controls.Add(this.upcoming_box);
            this.Controls.Add(this.prevsongs_label);
            this.Controls.Add(this.recent_box);
            this.Name = "ShuffleQueueSettingsControl";
            this.Size = new System.Drawing.Size(205, 186);
            ((System.ComponentModel.ISupportInitialize)(this.upcoming_box)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.recent_box)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title_label;
        private System.Windows.Forms.Label nextsongs_label;
        private System.Windows.Forms.NumericUpDown upcoming_box;
        private System.Windows.Forms.Label prevsongs_label;
        private System.Windows.Forms.NumericUpDown recent_box;
        private System.Windows.Forms.Label source_label;
        private System.Windows.Forms.ComboBox playlist_box;

    }
}
