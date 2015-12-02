using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.Lib;

namespace KoPlayer.Forms
{
    public partial class RatingFilterPlaylistPopup : Form
    {
        private RatingFilterInfo result;

        /// <summary>
        /// This is a window meant for creating a new filtered playlist
        /// </summary>
        public RatingFilterPlaylistPopup()
        {
            InitializeComponent();
            this.ratingBox1.Value = 3;
        }

        /// <summary>
        /// This is a window meant for editing an existing filtered playlist
        /// </summary>
        /// <param name="pl"></param>
        public RatingFilterPlaylistPopup(RatingFilterPlaylist pl)
        {
            InitializeComponent();

            this.Text = "Edit rating filter playlist";
            this.ratingBox1.Value = pl.AllowedRating;
            this.includehigher_checkbox.Checked = pl.IncludeHigher;
            this.create_button.Text = "Save";
        }

        private void create_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.result = new RatingFilterInfo((int)this.ratingBox1.Value, this.includehigher_checkbox.Checked);
            this.Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.No;
            this.Close();
        }

        public RatingFilterInfo GetResult()
        {
            return this.result;
        }

        private void ratingBox1_ValueChanged_1(object sender, EventArgs e)
        {
            if (ratingBox1.Value > 5)
                ratingBox1.Value = 5;
            else if (ratingBox1.Value < 0)
                ratingBox1.Value = 0;
        }

        public void SetStartPosition()
        {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
                (int)(Cursor.Position.X - 0.5 * this.Size.Width),
                (int)(Cursor.Position.Y - 0.5 * this.Size.Width));
        }
    }

    public struct RatingFilterInfo
    {
        public int AllowedRating;
        public bool IncludeHigher;
        public RatingFilterInfo(int allowedRating, bool andAbove)
        {
            this.AllowedRating = allowedRating;
            this.IncludeHigher = andAbove;
        }
    }
}
