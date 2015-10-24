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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.title_label = new System.Windows.Forms.Label();
            this.artist_label = new System.Windows.Forms.Label();
            this.album_label = new System.Windows.Forms.Label();
            this.length_label = new System.Windows.Forms.Label();
            this.rating_label = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 57);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(120, 120);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // title_label
            // 
            this.title_label.AutoEllipsis = true;
            this.title_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title_label.Location = new System.Drawing.Point(11, 9);
            this.title_label.Name = "title_label";
            this.title_label.Size = new System.Drawing.Size(180, 15);
            this.title_label.TabIndex = 2;
            this.title_label.Text = "label1";
            // 
            // artist_label
            // 
            this.artist_label.AutoEllipsis = true;
            this.artist_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.artist_label.Location = new System.Drawing.Point(11, 24);
            this.artist_label.Name = "artist_label";
            this.artist_label.Size = new System.Drawing.Size(180, 15);
            this.artist_label.TabIndex = 3;
            this.artist_label.Text = "label2";
            // 
            // album_label
            // 
            this.album_label.AutoEllipsis = true;
            this.album_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.album_label.Location = new System.Drawing.Point(11, 39);
            this.album_label.Name = "album_label";
            this.album_label.Size = new System.Drawing.Size(180, 15);
            this.album_label.TabIndex = 4;
            this.album_label.Text = "label3";
            // 
            // length_label
            // 
            this.length_label.AutoEllipsis = true;
            this.length_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.length_label.Location = new System.Drawing.Point(138, 57);
            this.length_label.Name = "length_label";
            this.length_label.Size = new System.Drawing.Size(48, 23);
            this.length_label.TabIndex = 5;
            this.length_label.Text = "label4";
            // 
            // rating_label
            // 
            this.rating_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rating_label.Location = new System.Drawing.Point(138, 90);
            this.rating_label.Name = "rating_label";
            this.rating_label.Size = new System.Drawing.Size(21, 87);
            this.rating_label.TabIndex = 6;
            this.rating_label.Text = "★★★★★";
            // 
            // SongInfoPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(203, 183);
            this.Controls.Add(this.rating_label);
            this.Controls.Add(this.length_label);
            this.Controls.Add(this.album_label);
            this.Controls.Add(this.artist_label);
            this.Controls.Add(this.title_label);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SongInfoPopup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label title_label;
        private System.Windows.Forms.Label artist_label;
        private System.Windows.Forms.Label album_label;
        private System.Windows.Forms.Label length_label;
        private System.Windows.Forms.Label rating_label;
    }
}