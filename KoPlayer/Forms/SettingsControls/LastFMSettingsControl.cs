using System;
using System.Linq;
using System.Windows.Forms;

namespace KoPlayer.Forms.SettingsControls
{
    public partial class LastFMSettingsControl : UserControl
    {
        private Settings settings;
        private LastfmHandler lfmHandler;

        public LastFMSettingsControl(Settings settings, LastfmHandler lfmHandler)
        {
            this.settings = settings;
            this.lfmHandler = lfmHandler;

            InitializeComponent();

            username_box.Text = lfmHandler.UserName;

            enable_checkbox.Checked = settings.ScrobblingEnabled;

            lfmHandler.StatusChanged += lfmHandler_StatusChanged;
            status_label.Text = lfmHandler.Status;
        }

        private void lfmHandler_StatusChanged(object sender, EventArgs e)
        {
            status_label.Text = lfmHandler.Status;

            if (lfmHandler.Status == "Connected")
            {
                int len = password_box.Text.Length;
                for (int i = 0; i < len; i++)
                    password_box.Text.ToArray()[i] = '*';
            }
        }

        private void enable_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            var checkbox = sender as CheckBox;
            settings.ScrobblingEnabled = checkbox.Checked;

            if (checkbox.Checked)
                lfmHandler.ResumeSessionAsync();
            else
                lfmHandler.Initialize();
        }

        private void connect_button_Click(object sender, EventArgs e)
        {
            enable_checkbox.Checked = true;
            lfmHandler.LoginAsync(username_box.Text, password_box.Text);
        }
    }
}
