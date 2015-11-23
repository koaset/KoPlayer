﻿using System;
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
        Settings settings;

        public LastFMSettingsControl(Settings settings)
        {
            this.settings = settings;

            InitializeComponent();

        }

        private void connect_button_Click(object sender, EventArgs e)
        {
            string apiKey = "derp";
            string apiSecret = "herp";
            var handler = new LastFMHandler(apiKey, apiSecret);
        }
    }
}
