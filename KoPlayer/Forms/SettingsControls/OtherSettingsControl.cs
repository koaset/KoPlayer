using System.Windows.Forms;

namespace KoPlayer.Forms.SettingsControls
{
    public partial class OtherSettingsControl : UserControl
    {
        Settings settings;

        public OtherSettingsControl(Settings settings)
        {
            this.settings = settings;

            InitializeComponent();

            nowPlayingEnabledcheckBox.Checked = settings.SaveCurrentlyPlayingToFile;
            formatTextBox.Enabled = settings.SaveCurrentlyPlayingToFile;
            formatTextBox.Text = settings.CurrentlyPlayingBaseString;
        }

        private void nowPlayingEnabledcheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            formatTextBox.Enabled = nowPlayingEnabledcheckBox.Checked;
            settings.SaveCurrentlyPlayingToFile = nowPlayingEnabledcheckBox.Checked;
        }

        private void formatTextBox_TextChanged(object sender, System.EventArgs e)
        {
            settings.CurrentlyPlayingBaseString = formatTextBox.Text;
        }
    }
}
