using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.PlayLists;

namespace KoPlayer.Forms.SettingsControls
{
    public partial class PartyMixSettingsControl : UserControl
    {
        Settings settings;
        List<string> playListNames;

        public PartyMixSettingsControl(Settings settings, List<IPlayList> playLists)
        {
            this.settings = settings;
            this.playListNames = new List<string>();
            foreach (IPlayList pl in playLists)
                if (pl.Name != "Party Mix")
                    this.playListNames.Add(pl.Name);

            InitializeComponent();

            this.recent_box.Value = settings.Partymix_NumPrevious;
            this.upcoming_box.Value = settings.Partymix_NumNext;

            foreach (string name in this.playListNames)
                this.playlist_box.Items.Add(name);
            this.playlist_box.SelectedIndex = this.playListNames.IndexOf(settings.Partymix_SourcePlayListName);
        }

        private void playlist_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.settings.Partymix_SourcePlayListName = this.playlist_box.Text;
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
