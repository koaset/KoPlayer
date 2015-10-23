namespace KoPlayer.Forms
{
    partial class RatingFilterPlaylistPopup
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
            this.label1 = new System.Windows.Forms.Label();
            this.ratingBox1 = new KoPlayer.Forms.RatingBox();
            this.includehigher_checkbox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.ratingBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // create_button
            // 
            this.create_button.Location = new System.Drawing.Point(12, 88);
            this.create_button.Name = "create_button";
            this.create_button.Size = new System.Drawing.Size(75, 23);
            this.create_button.TabIndex = 0;
            this.create_button.Text = "Create";
            this.create_button.UseVisualStyleBackColor = true;
            this.create_button.Click += new System.EventHandler(this.create_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(140, 88);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(75, 23);
            this.cancel_button.TabIndex = 1;
            this.cancel_button.Text = "Cancel";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Add songs of rating";
            // 
            // ratingBox1
            // 
            this.ratingBox1.Location = new System.Drawing.Point(116, 16);
            this.ratingBox1.Name = "ratingBox1";
            this.ratingBox1.Size = new System.Drawing.Size(102, 20);
            this.ratingBox1.TabIndex = 4;
            this.ratingBox1.ValueChanged += new System.EventHandler(this.ratingBox1_ValueChanged_1);
            // 
            // includehigher_checkbox
            // 
            this.includehigher_checkbox.AutoSize = true;
            this.includehigher_checkbox.Location = new System.Drawing.Point(12, 52);
            this.includehigher_checkbox.Name = "includehigher_checkbox";
            this.includehigher_checkbox.Size = new System.Drawing.Size(165, 17);
            this.includehigher_checkbox.TabIndex = 6;
            this.includehigher_checkbox.Text = "Include songs of higher rating";
            this.includehigher_checkbox.UseVisualStyleBackColor = true;
            // 
            // CreateRatingFilterPlaylistPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(227, 123);
            this.Controls.Add(this.includehigher_checkbox);
            this.Controls.Add(this.ratingBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.create_button);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CreateRatingFilterPlaylistPopup";
            this.Text = "Rating Filter Playlist";
            ((System.ComponentModel.ISupportInitialize)(this.ratingBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button create_button;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.Label label1;
        private RatingBox ratingBox1;
        private System.Windows.Forms.CheckBox includehigher_checkbox;
    }
}