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
    public partial class HotkeySettingsControl : UserControl
    {
        Settings settings;
        Keys currentKey = Keys.Shift;

        public HotkeySettingsControl(Settings settings)
        {
            this.settings = settings;

            InitializeComponent();

            foreach (Keys key in settings.RatingHotkeys)
            {
                ListViewItem newItem = new ListViewItem();
                newItem.Text = "Rate " + Array.IndexOf(settings.RatingHotkeys, key);
                newItem.SubItems.Add("Ctrl + " + (char)((int)key));
                command_list.Items.Add(newItem);
            }
        }

        private void command_box_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            if (Char.IsDigit((char)e.KeyCode) || Char.IsLetter((char)e.KeyCode))
            {
                command_box.Text = "Ctrl + " + (char)e.KeyCode;
                currentKey = e.KeyCode;
            }
        }

        private void setkey_button_Click(object sender, EventArgs e)
        {
            if (command_list.SelectedItems.Count == 1)
            {
                if (currentKey != Keys.Shift)
                {
                    command_list.SelectedItems[0].SubItems[1].Text = "Ctrl + " + (char)currentKey;
                    settings.RatingHotkeys[command_list.SelectedItems[0].Index] = currentKey;
                }
            }
        }
    }
}
