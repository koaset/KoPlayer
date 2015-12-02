using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.Lib;

namespace KoPlayer.Forms.SettingsControls
{
    public partial class ShuffleQueueSettingsControl : UserControl
    {
        Settings settings;
        List<string> playlistNames;

        public ShuffleQueueSettingsControl(Settings settings, List<IPlaylist> playlists)
        {
            this.settings = settings;
            this.playlistNames = new List<string>();
            foreach (IPlaylist pl in playlists)
                if (pl.Name != "Shuffle Queue")
                    this.playlistNames.Add(pl.Name);

            InitializeComponent();

            this.recent_box.Value = settings.Shufflequeue_NumPrevious;
            this.upcoming_box.Value = settings.Shufflequeue_NumNext;

            foreach (string name in this.playlistNames)
                this.playlist_box.Items.Add(name);
            this.playlist_box.SelectedIndex = this.playlistNames.IndexOf(settings.Shufflequeue_SourcePlaylistName);
        }

        private void playlist_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.settings.Shufflequeue_SourcePlaylistName = this.playlist_box.Text;
        }

        private void recent_box_ValueChanged(object sender, EventArgs e)
        {
            this.settings.Shufflequeue_NumPrevious = (int)this.recent_box.Value;
        }

        private void upcoming_box_ValueChanged(object sender, EventArgs e)
        {
            this.settings.Shufflequeue_NumNext = (int)this.upcoming_box.Value;
        }
    }
}
