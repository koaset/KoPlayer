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
    public partial class SongInfoPopup : Form
    {
        private Timer popupTimer;

        /// <summary>
        /// Interval in ms
        /// </summary>
        /// <param name="song"></param>
        /// <param name="interval"></param>
        private SongInfoPopup(Song song, Image albumArt, int interval)
        {
            InitializeComponent();

            Screen currentScreen = Screen.PrimaryScreen;
            this.Left = currentScreen.WorkingArea.Right - this.Width;
            this.Top = currentScreen.WorkingArea.Bottom - this.Height;

            title_label.Text = song.Title;
            artist_label.Text = song.Artist;
            album_label.Text = song.Album;
            length_label.Text = song.LengthString;
            rating_label.Text = song.RatingString;
            pictureBox1.Image = albumArt;

            this.Show();

            popupTimer = new Timer();
            popupTimer.Interval = interval;
            popupTimer.Tick += popupTimer_Tick;
            popupTimer.Start();
        }

        private void popupTimer_Tick(object sender, EventArgs e)
        {
            popupTimer.Stop();
            popupTimer.Dispose();
            popupTimer = null;
            this.Close();
            this.Dispose();
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Time in ms
        /// </summary>
        /// <param name="s"></param>
        /// <param name="time"></param>
        public static void ShowPopup(Song s, Image albumArt, int time)
        {
            SongInfoPopup popup = new SongInfoPopup(s, albumArt, time);
        }
    }
}
