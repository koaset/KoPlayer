using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.PlayLists;

namespace KoPlayer.Forms
{
    public partial class SongInfoPopup : Form
    {
        private Song song;

        public SongInfoPopup(Song song)
        {
            this.song = song;
            InitializeComponent();
        }

        private void SongInfoPopup_Load(object sender, EventArgs e)
        {
            this.title_box.Text = song.Title;
            this.artist_box.Text = song.Artist;
            this.album_box.Text = song.Album;
            this.length_box.Text = song.Length;
            this.tracknr_box.Text = song.TrackNumber.ToString();
            this.discnr_box.Text = song.DiscNumber.ToString();
            this.genre_box.Text = song.Genre;
            this.rating_numupdownstring.Value = song.Rating;
            this.playcount_box.Text = song.PlayCount.ToString();
            this.dateadded_box.Text = song.DateAdded.ToShortDateString() + " " + song.DateAdded.ToShortTimeString();
            if (song.LastPlayed.Ticks != 0)
                this.lastplayed_box.Text = song.LastPlayed.ToShortDateString() + " " + song.LastPlayed.ToShortTimeString();
            this.path_box.Text = song.Path;
        }

        private void ok_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void rating_numupdownstring_Enter(object sender, EventArgs e)
        {

        }
    }
}
