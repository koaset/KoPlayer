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
    public partial class LastFMSettingsControl : UserControl
    {
        private Settings settings;
        private LastfmHandler lfmHandler;

        public LastFMSettingsControl(Settings settings, LastfmHandler lfmHandler)
        {
            this.settings = settings;
            this.lfmHandler = lfmHandler;
            
            lfmHandler.StatusChanged += lfmHandler_StatusChanged;

            InitializeComponent();

            username_box.Text = lfmHandler.SessionUserName;

            enable_checkbox.Checked = settings.ScrobblingEnabled;

            if (enable_checkbox.Checked)
                lfmHandler.TryResumeSession();
            else
                lfmHandler.Initialize();

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
                lfmHandler.TryResumeSession();
            else
                lfmHandler.Initialize();
        }

        private void connect_button_Click(object sender, EventArgs e)
        {
            enable_checkbox.Checked = true;
            lfmHandler.TryLoginAsync(username_box.Text, password_box.Text);
        }
    }
}
