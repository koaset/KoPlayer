using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.Playlists;

namespace KoPlayer.Forms
{
    public delegate void SavePlayingSongEventHandler(object sender, SavePlayingSongEventArgs e);

    public partial class SongPropertiesWindow : Form
    {
        public event SavePlayingSongEventHandler SavePlayingSong;

        private Song song;
        private int currentIndex;
        private IPlaylist currentPlaylist;
        private Library library;
        private MainForm mainForm;

        public SongPropertiesWindow(MainForm mainForm, Song song, int clickedIndex, IPlaylist currentPlaylist, Library library)
        {
            this.mainForm = mainForm;
            this.song = song;
            this.currentIndex = clickedIndex;
            this.currentPlaylist = currentPlaylist;
            this.library = library;

            InitializeComponent();
        }

        private void SongInfoPopup_Load(object sender, EventArgs e)
        {
            LoadSong();
        }

        private void LoadSong()
        {
            this.title_box.Text = this.song.Title;
            this.artist_box.Text = this.song.Artist;
            this.album_box.Text = this.song.Album;
            this.length_box.Text = this.song.LengthString;
            this.tracknr_box.Text = this.song.TrackNumber.ToString();
            this.discnr_box.Text = this.song.DiscNumber.ToString();
            this.genre_box.Text = this.song.Genre;
            this.rating_numupdownstring.Value = this.song.Rating;
            this.playcount_box.Text = this.song.PlayCount.ToString();
            this.dateadded_box.Text = this.song.DateAdded.ToShortDateString() + " " + this.song.DateAdded.ToShortTimeString();
            if (this.song.LastPlayed.Ticks != 0)
                this.lastplayed_box.Text = this.song.LastPlayed.ToShortDateString() + " " + this.song.LastPlayed.ToShortTimeString();
            this.path_box.Text = this.song.Path;
        }

        #region Button events
        private void ok_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.SaveCurrentSong();
            this.Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void next_button_Click(object sender, EventArgs e)
        {
            SaveCurrentSong();
            GetNextOrPrevious(true);
            LoadSong();
        }

        private void previous_button_Click(object sender, EventArgs e)
        {
            SaveCurrentSong();
            GetNextOrPrevious(false);
            LoadSong();
        }
        #endregion

        private void SaveCurrentSong()
        {
            this.song.Rating = (int)this.rating_numupdownstring.Value;

            this.song.Title = this.title_box.Text;
            this.song.Artist = this.artist_box.Text;
            this.song.Album = this.album_box.Text;
            this.song.TrackNumber = Convert.ToInt32(this.tracknr_box.Text);
            this.song.DiscNumber = Convert.ToInt32(this.discnr_box.Text);
            this.song.Genre = this.genre_box.Text;
            this.song.Rating = (int)this.rating_numupdownstring.Value;
            
            if (this.song != this.mainForm.CurrentlyPlaying)
                this.song.SaveTags();
            else
                OnSavePlayingSong(this.song);

            this.library.UpdateSongInfo(this.song);
            
        }

        private void OnSavePlayingSong(Song song)
        {
            if (SavePlayingSong != null)
                SavePlayingSong(this, new SavePlayingSongEventArgs(song));
        }

        private void GetNextOrPrevious(bool getNext)
        {
            //Index juggling required as GetNext() and GetPrevious() increments playing playlist index
            //The methods also jump to the beginning/end of a playlist if you pass the end/beginning
            //So this way is probably more convenient
            int oldIndex = currentPlaylist.CurrentIndex;
            currentPlaylist.CurrentIndex = this.currentIndex;

            if (getNext)
                song = currentPlaylist.GetNext();
            else
                song = currentPlaylist.GetPrevious();

            this.currentIndex = currentPlaylist.CurrentIndex;
            currentPlaylist.CurrentIndex = oldIndex;
        }

        private void nr_box_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }
    }

    public class SavePlayingSongEventArgs : EventArgs
    {
        public Song savingSong { get; set; }
        public SavePlayingSongEventArgs(Song savingSong)
            : base()
        {
            this.savingSong = savingSong;
        }
    }
}
