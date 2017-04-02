using System.Windows.Forms;

namespace KoPlayer.Forms.SettingsControls
{
    public partial class AudioSettingsControl : UserControl
    {
        Settings settings;

        public AudioSettingsControl(Settings settings)
        {
            this.settings = settings;

            InitializeComponent();
            
            foreach (var device in MusicPlayer.PlaybackDevices)
            {
                var index = devices_comboBox.Items.Add(device.FriendlyName);

                if (device.FriendlyName == settings.DeviceName)
                {                    
                    settings.DeviceName = device.FriendlyName;
                    devices_comboBox.SelectedIndex = index;
                }
            }
        }

        private void devices_comboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            settings.DeviceName = devices_comboBox.Text;
        }
    }
}
