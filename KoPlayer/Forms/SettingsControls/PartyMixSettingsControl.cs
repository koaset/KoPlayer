using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.Playlists;

namespace KoPlayer.Forms.SettingsControls
{
    public partial class PartyMixSettingsControl : UserControl
    {
        Settings settings;
        List<string> playlistNames;

        public PartyMixSettingsControl(Settings settings, List<IPlaylist> playlists)
        {
            this.settings = settings;
            this.playlistNames = new List<string>();
            foreach (IPlaylist pl in playlists)
                if (pl.Name != "Party Mix")
                    this.playlistNames.Add(pl.Name);

            InitializeComponent();

            this.recent_box.Value = settings.Partymix_NumPrevious;
            this.upcoming_box.Value = settings.Partymix_NumNext;

            foreach (string name in this.playlistNames)
                this.playlist_box.Items.Add(name);
            this.playlist_box.SelectedIndex = this.playlistNames.IndexOf(settings.Partymix_SourcePlaylistName);
        }

        private void playlist_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.settings.Partymix_SourcePlaylistName = this.playlist_box.Text;
        }

        private void recent_box_ValueChanged(object sender, EventArgs e)
        {
            this.settings.Partymix_NumPrevious = (int)this.recent_box.Value;
        }

        private void upcoming_box_ValueChanged(object sender, EventArgs e)
        {
            this.settings.Partymix_NumNext = (int)this.upcoming_box.Value;
        }
    }
}
