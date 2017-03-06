using System;
using System.Windows.Forms;
using KoPlayer.Lib;

namespace KoPlayer.Forms
{
    public delegate void SavePlayingSongEventHandler(object sender, SavePlayingSongEventArgs e);
    public delegate void SongChangedEventHandler(object sender, SongChangedEventArgs e);

    public partial class SongPropertiesWindow : Form
    {
        public event SavePlayingSongEventHandler SavePlayingSong;
        public event SongChangedEventHandler SongChanged;

        public Song Song { get { return song; } }

        private Song song;
        private int currentIndex;
        private PlaylistBase currentPlaylist;
        private Library library;
        private MainForm mainForm;
        private Control lastFocusedTextBox;

        public SongPropertiesWindow(MainForm mainForm, Song song, int clickedIndex, PlaylistBase currentPlaylist, Library library)
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
            foreach (Control control in Controls)
                control.Enter += ControlReceivedFocus;

            LoadSong();
        }

        private void ControlReceivedFocus(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(TextBox) || sender.GetType() == typeof(Controls.RatingBox))
                lastFocusedTextBox = sender as Control;
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
            SetFocusOnSwitchSong();
        }

        private void previous_button_Click(object sender, EventArgs e)
        {
            SaveCurrentSong();
            GetNextOrPrevious(false);
            LoadSong();
            SetFocusOnSwitchSong();
        }
        #endregion

        private void SetFocusOnSwitchSong()
        {
            if (lastFocusedTextBox != null)
                lastFocusedTextBox.Focus();
            else
                title_box.Focus();
        }

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
                this.song.Save();
            else
                OnSavePlayingSong(this.song);

            OnSongChanged(this.song);

            this.library.UpdateSongInfo(this.song);
            
        }

        private void OnSongChanged(Song song)
        {
            if (SongChanged != null)
                SongChanged(this, new SongChangedEventArgs(song));
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

        private void SongPropertiesWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    e.Handled = true;
                    ok_button_Click(this, new EventArgs());
                    break;
                case Keys.Escape:
                    e.Handled = true;
                    cancel_button_Click(this, new EventArgs());
                    break;
            }
        }
    }

    public class SavePlayingSongEventArgs : EventArgs
    {
        public Song SavingSong { get; set; }
        public SavePlayingSongEventArgs(Song savingSong)
            : base()
        {
            SavingSong = savingSong;
        }
    }

    public class SongChangedEventArgs : EventArgs
    {
        public Song ChangedSong { get; set; }
        public SongChangedEventArgs(Song changedSong)
            : base()
        {
            ChangedSong = changedSong;
        }
    }
}
