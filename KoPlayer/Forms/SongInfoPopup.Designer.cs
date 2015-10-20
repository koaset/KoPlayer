namespace KoPlayer.Forms
{
    partial class SongInfoPopup
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
            this.title_box = new System.Windows.Forms.TextBox();
            this.artist_box = new System.Windows.Forms.TextBox();
            this.album_box = new System.Windows.Forms.TextBox();
            this.genre_box = new System.Windows.Forms.TextBox();
            this.path_box = new System.Windows.Forms.TextBox();
            this.tracknr_box = new System.Windows.Forms.TextBox();
            this.discnr_box = new System.Windows.Forms.TextBox();
            this.title_label = new System.Windows.Forms.Label();
            this.artist_label = new System.Windows.Forms.Label();
            this.album_label = new System.Windows.Forms.Label();
            this.tracknr_label = new System.Windows.Forms.Label();
            this.path_label = new System.Windows.Forms.Label();
            this.discnr_label = new System.Windows.Forms.Label();
            this.genre_label = new System.Windows.Forms.Label();
            this.length_label = new System.Windows.Forms.Label();
            this.length_box = new System.Windows.Forms.TextBox();
            this.rating_label = new System.Windows.Forms.Label();
            this.playcount_label = new System.Windows.Forms.Label();
            this.playcount_box = new System.Windows.Forms.TextBox();
            this.dateadded_label = new System.Windows.Forms.Label();
            this.dateadded_box = new System.Windows.Forms.TextBox();
            this.lastplayed_label = new System.Windows.Forms.Label();
            this.lastplayed_box = new System.Windows.Forms.TextBox();
            this.ok_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.previous_button = new System.Windows.Forms.Button();
            this.next_button = new System.Windows.Forms.Button();
            this.rating_numupdownstring = new KoPlayer.Forms.RatingBox();
            ((System.ComponentModel.ISupportInitialize)(this.rating_numupdownstring)).BeginInit();
            this.SuspendLayout();
            // 
            // title_box
            // 
            this.title_box.Location = new System.Drawing.Point(12, 28);
            this.title_box.Name = "title_box";
            this.title_box.Size = new System.Drawing.Size(369, 20);
            this.title_box.TabIndex = 0;
            // 
            // artist_box
            // 
            this.artist_box.Location = new System.Drawing.Point(12, 69);
            this.artist_box.Name = "artist_box";
            this.artist_box.Size = new System.Drawing.Size(369, 20);
            this.artist_box.TabIndex = 1;
            // 
            // album_box
            // 
            this.album_box.Location = new System.Drawing.Point(12, 114);
            this.album_box.Name = "album_box";
            this.album_box.Size = new System.Drawing.Size(369, 20);
            this.album_box.TabIndex = 2;
            // 
            // genre_box
            // 
            this.genre_box.Location = new System.Drawing.Point(203, 156);
            this.genre_box.Name = "genre_box";
            this.genre_box.Size = new System.Drawing.Size(178, 20);
            this.genre_box.TabIndex = 5;
            // 
            // path_box
            // 
            this.path_box.Location = new System.Drawing.Point(12, 288);
            this.path_box.Name = "path_box";
            this.path_box.ReadOnly = true;
            this.path_box.Size = new System.Drawing.Size(369, 20);
            this.path_box.TabIndex = 5;
            this.path_box.TabStop = false;
            // 
            // tracknr_box
            // 
            this.tracknr_box.Location = new System.Drawing.Point(98, 156);
            this.tracknr_box.Name = "tracknr_box";
            this.tracknr_box.Size = new System.Drawing.Size(33, 20);
            this.tracknr_box.TabIndex = 3;
            // 
            // discnr_box
            // 
            this.discnr_box.Location = new System.Drawing.Point(155, 156);
            this.discnr_box.Name = "discnr_box";
            this.discnr_box.Size = new System.Drawing.Size(30, 20);
            this.discnr_box.TabIndex = 4;
            // 
            // title_label
            // 
            this.title_label.AutoSize = true;
            this.title_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title_label.Location = new System.Drawing.Point(9, 12);
            this.title_label.Name = "title_label";
            this.title_label.Size = new System.Drawing.Size(32, 13);
            this.title_label.TabIndex = 10;
            this.title_label.Text = "Title";
            // 
            // artist_label
            // 
            this.artist_label.AutoSize = true;
            this.artist_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.artist_label.Location = new System.Drawing.Point(9, 53);
            this.artist_label.Name = "artist_label";
            this.artist_label.Size = new System.Drawing.Size(36, 13);
            this.artist_label.TabIndex = 11;
            this.artist_label.Text = "Artist";
            // 
            // album_label
            // 
            this.album_label.AutoSize = true;
            this.album_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.album_label.Location = new System.Drawing.Point(9, 98);
            this.album_label.Name = "album_label";
            this.album_label.Size = new System.Drawing.Size(41, 13);
            this.album_label.TabIndex = 12;
            this.album_label.Text = "Album";
            // 
            // tracknr_label
            // 
            this.tracknr_label.AutoSize = true;
            this.tracknr_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tracknr_label.Location = new System.Drawing.Point(95, 140);
            this.tracknr_label.Name = "tracknr_label";
            this.tracknr_label.Size = new System.Drawing.Size(52, 13);
            this.tracknr_label.TabIndex = 13;
            this.tracknr_label.Text = "Track #";
            // 
            // path_label
            // 
            this.path_label.AutoSize = true;
            this.path_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.path_label.Location = new System.Drawing.Point(9, 272);
            this.path_label.Name = "path_label";
            this.path_label.Size = new System.Drawing.Size(33, 13);
            this.path_label.TabIndex = 14;
            this.path_label.Text = "Path";
            // 
            // discnr_label
            // 
            this.discnr_label.AutoSize = true;
            this.discnr_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.discnr_label.Location = new System.Drawing.Point(152, 140);
            this.discnr_label.Name = "discnr_label";
            this.discnr_label.Size = new System.Drawing.Size(44, 13);
            this.discnr_label.TabIndex = 15;
            this.discnr_label.Text = "Disc #";
            // 
            // genre_label
            // 
            this.genre_label.AutoSize = true;
            this.genre_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.genre_label.Location = new System.Drawing.Point(200, 140);
            this.genre_label.Name = "genre_label";
            this.genre_label.Size = new System.Drawing.Size(41, 13);
            this.genre_label.TabIndex = 16;
            this.genre_label.Text = "Genre";
            // 
            // length_label
            // 
            this.length_label.AutoSize = true;
            this.length_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.length_label.Location = new System.Drawing.Point(9, 140);
            this.length_label.Name = "length_label";
            this.length_label.Size = new System.Drawing.Size(46, 13);
            this.length_label.TabIndex = 18;
            this.length_label.Text = "Length";
            // 
            // length_box
            // 
            this.length_box.Location = new System.Drawing.Point(12, 156);
            this.length_box.Name = "length_box";
            this.length_box.ReadOnly = true;
            this.length_box.Size = new System.Drawing.Size(72, 20);
            this.length_box.TabIndex = 17;
            this.length_box.TabStop = false;
            // 
            // rating_label
            // 
            this.rating_label.AutoSize = true;
            this.rating_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rating_label.Location = new System.Drawing.Point(9, 182);
            this.rating_label.Name = "rating_label";
            this.rating_label.Size = new System.Drawing.Size(44, 13);
            this.rating_label.TabIndex = 20;
            this.rating_label.Text = "Rating";
            // 
            // playcount_label
            // 
            this.playcount_label.AutoSize = true;
            this.playcount_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playcount_label.Location = new System.Drawing.Point(200, 182);
            this.playcount_label.Name = "playcount_label";
            this.playcount_label.Size = new System.Drawing.Size(67, 13);
            this.playcount_label.TabIndex = 22;
            this.playcount_label.Text = "Play count";
            // 
            // playcount_box
            // 
            this.playcount_box.Location = new System.Drawing.Point(203, 198);
            this.playcount_box.Name = "playcount_box";
            this.playcount_box.ReadOnly = true;
            this.playcount_box.Size = new System.Drawing.Size(64, 20);
            this.playcount_box.TabIndex = 21;
            this.playcount_box.TabStop = false;
            // 
            // dateadded_label
            // 
            this.dateadded_label.AutoSize = true;
            this.dateadded_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateadded_label.Location = new System.Drawing.Point(9, 226);
            this.dateadded_label.Name = "dateadded_label";
            this.dateadded_label.Size = new System.Drawing.Size(73, 13);
            this.dateadded_label.TabIndex = 24;
            this.dateadded_label.Text = "Date added";
            // 
            // dateadded_box
            // 
            this.dateadded_box.Location = new System.Drawing.Point(12, 242);
            this.dateadded_box.Name = "dateadded_box";
            this.dateadded_box.ReadOnly = true;
            this.dateadded_box.Size = new System.Drawing.Size(173, 20);
            this.dateadded_box.TabIndex = 23;
            this.dateadded_box.TabStop = false;
            // 
            // lastplayed_label
            // 
            this.lastplayed_label.AutoSize = true;
            this.lastplayed_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lastplayed_label.Location = new System.Drawing.Point(200, 226);
            this.lastplayed_label.Name = "lastplayed_label";
            this.lastplayed_label.Size = new System.Drawing.Size(72, 13);
            this.lastplayed_label.TabIndex = 26;
            this.lastplayed_label.Text = "Last played";
            // 
            // lastplayed_box
            // 
            this.lastplayed_box.Location = new System.Drawing.Point(203, 242);
            this.lastplayed_box.Name = "lastplayed_box";
            this.lastplayed_box.ReadOnly = true;
            this.lastplayed_box.Size = new System.Drawing.Size(178, 20);
            this.lastplayed_box.TabIndex = 25;
            this.lastplayed_box.TabStop = false;
            // 
            // ok_button
            // 
            this.ok_button.Location = new System.Drawing.Point(225, 327);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(75, 23);
            this.ok_button.TabIndex = 27;
            this.ok_button.Text = "Ok";
            this.ok_button.UseVisualStyleBackColor = true;
            this.ok_button.Click += new System.EventHandler(this.ok_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(306, 327);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 28;
            this.cancel_button.Text = "Cancel";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // previous_button
            // 
            this.previous_button.Location = new System.Drawing.Point(12, 327);
            this.previous_button.Name = "previous_button";
            this.previous_button.Size = new System.Drawing.Size(75, 23);
            this.previous_button.TabIndex = 29;
            this.previous_button.Text = "Previous";
            this.previous_button.UseVisualStyleBackColor = true;
            // 
            // next_button
            // 
            this.next_button.Location = new System.Drawing.Point(93, 327);
            this.next_button.Name = "next_button";
            this.next_button.Size = new System.Drawing.Size(75, 23);
            this.next_button.TabIndex = 30;
            this.next_button.Text = "Next";
            this.next_button.UseVisualStyleBackColor = true;
            // 
            // rating_numupdownstring
            // 
            this.rating_numupdownstring.BackColor = System.Drawing.Color.White;
            this.rating_numupdownstring.Location = new System.Drawing.Point(12, 198);
            this.rating_numupdownstring.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.rating_numupdownstring.Name = "rating_numupdownstring";
            this.rating_numupdownstring.ReadOnly = true;
            this.rating_numupdownstring.Size = new System.Drawing.Size(120, 20);
            this.rating_numupdownstring.TabIndex = 31;
            this.rating_numupdownstring.TabStop = false;
            // 
            // SongInfoPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 375);
            this.Controls.Add(this.rating_numupdownstring);
            this.Controls.Add(this.next_button);
            this.Controls.Add(this.previous_button);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.ok_button);
            this.Controls.Add(this.lastplayed_label);
            this.Controls.Add(this.lastplayed_box);
            this.Controls.Add(this.dateadded_label);
            this.Controls.Add(this.dateadded_box);
            this.Controls.Add(this.playcount_label);
            this.Controls.Add(this.playcount_box);
            this.Controls.Add(this.rating_label);
            this.Controls.Add(this.length_label);
            this.Controls.Add(this.length_box);
            this.Controls.Add(this.genre_label);
            this.Controls.Add(this.discnr_label);
            this.Controls.Add(this.path_label);
            this.Controls.Add(this.tracknr_label);
            this.Controls.Add(this.album_label);
            this.Controls.Add(this.artist_label);
            this.Controls.Add(this.title_label);
            this.Controls.Add(this.discnr_box);
            this.Controls.Add(this.tracknr_box);
            this.Controls.Add(this.path_box);
            this.Controls.Add(this.genre_box);
            this.Controls.Add(this.album_box);
            this.Controls.Add(this.artist_box);
            this.Controls.Add(this.title_box);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SongInfoPopup";
            this.Text = "Song Properties";
            this.Load += new System.EventHandler(this.SongInfoPopup_Load);
            ((System.ComponentModel.ISupportInitialize)(this.rating_numupdownstring)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox title_box;
        private System.Windows.Forms.TextBox artist_box;
        private System.Windows.Forms.TextBox album_box;
        private System.Windows.Forms.TextBox genre_box;
        private System.Windows.Forms.TextBox path_box;
        private System.Windows.Forms.TextBox tracknr_box;
        private System.Windows.Forms.TextBox discnr_box;
        private System.Windows.Forms.Label title_label;
        private System.Windows.Forms.Label artist_label;
        private System.Windows.Forms.Label album_label;
        private System.Windows.Forms.Label tracknr_label;
        private System.Windows.Forms.Label path_label;
        private System.Windows.Forms.Label discnr_label;
        private System.Windows.Forms.Label genre_label;
        private System.Windows.Forms.Label length_label;
        private System.Windows.Forms.TextBox length_box;
        private System.Windows.Forms.Label rating_label;
        private System.Windows.Forms.Label playcount_label;
        private System.Windows.Forms.TextBox playcount_box;
        private System.Windows.Forms.Label dateadded_label;
        private System.Windows.Forms.TextBox dateadded_box;
        private System.Windows.Forms.Label lastplayed_label;
        private System.Windows.Forms.TextBox lastplayed_box;
        private System.Windows.Forms.Button ok_button;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.Button previous_button;
        private System.Windows.Forms.Button next_button;
        private RatingBox rating_numupdownstring;
    }
}