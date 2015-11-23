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
    public partial class GeneralSettingsControl : UserControl
    {
        Settings settings;

        public GeneralSettingsControl(Settings settings)
        {
            this.settings = settings;

            InitializeComponent();

            startup_checkbox.Checked = settings.RunAtStartup;
            tray_checkbox.Checked = settings.MinimizeToTray;
            popup_checkbox.Checked = settings.PopupOnSongChange;
        }

        private void checkbox_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = sender as CheckBox;

            if (checkBox == startup_checkbox)
                settings.RunAtStartup = checkBox.Checked;
            else if (checkBox == tray_checkbox)
                settings.MinimizeToTray = checkBox.Checked;
            else if (checkBox == popup_checkbox)
                settings.PopupOnSongChange = checkBox.Checked;
            else
                throw new Exception("Unhandled event sender.");
        }
    }
}
