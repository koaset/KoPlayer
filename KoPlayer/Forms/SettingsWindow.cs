using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.Forms;

namespace KoPlayer.Forms
{
    public partial class SettingsWindow : Form
    {
        //private Settings tempSettings;
        private MainForm callingForm;

        public SettingsWindow(MainForm callingForm)
        {
            this.callingForm = callingForm;
            //this.tempSettings = Settings.Copy(callingForm.Settings);
            InitializeComponent();
        }

        private void PreferencesWindow_Load(object sender, EventArgs e)
        {

        }

        private void save_Button_Click(object sender, EventArgs e)
        {
            //callingForm.Settings = tempSettings;
            this.Close();
        }

        private void cancel_Button_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
