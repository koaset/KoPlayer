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

        public ShuffleQueueSettingsControl(Settings settings, List<PlaylistBase> playlists)
        {
            this.settings = settings;
            playlistNames = new List<string>();
            foreach (var pl in playlists)
                if (pl.Name != "Shuffle Queue")
                    playlistNames.Add(pl.Name);

            InitializeComponent();

            recent_box.Value = settings.Shufflequeue_NumPrevious;
            upcoming_box.Value = settings.Shufflequeue_NumNext;
            weighRating_checkBox.Checked = settings.Shufflequeue_WeighRating;

            foreach (string name in playlistNames)
                playlist_box.Items.Add(name);
            playlist_box.SelectedIndex = playlistNames.IndexOf(settings.Shufflequeue_SourcePlaylistName);
        }

        private void playlist_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            settings.Shufflequeue_SourcePlaylistName = playlist_box.Text;
        }

        private void recent_box_ValueChanged(object sender, EventArgs e)
        {
            settings.Shufflequeue_NumPrevious = (int)recent_box.Value;
        }

        private void upcoming_box_ValueChanged(object sender, EventArgs e)
        {
            settings.Shufflequeue_NumNext = (int)upcoming_box.Value;
        }

        private void weighRating_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            settings.Shufflequeue_WeighRating = weighRating_checkBox.Checked;
        }
    }
}
