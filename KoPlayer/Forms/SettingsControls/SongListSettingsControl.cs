using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KoPlayer.Forms.SettingsControls
{
    public partial class SongListSettingsControl : UserControl
    {
        Settings settings;

        public SongListSettingsControl(Settings settings)
        {
            this.settings = settings;

            InitializeComponent();
            this.rowheight_box.Minimum = Settings.RowHeightMin;
            this.rowheight_box.Maximum = Settings.RowHeightMax;
            this.rowheight_box.Value = settings.RowHeight;
            this.fontsize_box.Minimum = (decimal)Settings.FontSizeMin;
            this.fontsize_box.Maximum = (decimal)Settings.FontSizeMax;
            this.fontsize_box.Value = (decimal)settings.FontSize;
        }

        private void fontsize_box_Leave(object sender, EventArgs e)
        {
            this.settings.FontSize = (float)fontsize_box.Value;
        }

        private void rowheight_box_Leave(object sender, EventArgs e)
        {
            this.settings.RowHeight = (int)rowheight_box.Value;
        }
    }
}
