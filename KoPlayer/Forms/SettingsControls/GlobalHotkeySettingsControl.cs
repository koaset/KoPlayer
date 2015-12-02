using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.Lib;

namespace KoPlayer.Forms.SettingsControls
{
    public partial class GlobalHotkeySettingsControl : UserControl
    {
        Settings settings;
        ModifierKeys currentModifier;
        Keys currentKey = Keys.Shift;

        public GlobalHotkeySettingsControl(Settings settings)
        {
            this.settings = settings;

            InitializeComponent();

            for (int i = 0; i < settings.GlobalHotkeys.Length; i++)
            {
                ListViewItem newItem = new ListViewItem();
                newItem.Text = settings.GlobalHotkeyNames[i];
                string commandString = settings.GlobalHotkeys[i].Modifier + " + "
                    + settings.GlobalHotkeys[i].Key;
                newItem.SubItems.Add(commandString);
                command_list.Items.Add(newItem);
            }

            command_list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void command_box_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

                if (e.Modifiers == Keys.Control)
                    SetKeyAndModifier(Forms.ModifierKeys.Control, e.KeyCode);
                else if (e.Modifiers == Keys.Alt)
                    SetKeyAndModifier(Forms.ModifierKeys.Alt, e.KeyCode);
                else if (e.Modifiers == Keys.Shift)
                    SetKeyAndModifier(Forms.ModifierKeys.Shift, e.KeyCode);
        }

        private void SetKeyAndModifier(ModifierKeys modifier, Keys key)
        {
            this.currentModifier = modifier;
            this.currentKey = key;
            string keyString = key.ToString();

            if (key == Keys.End)
                keyString = "End";

            command_box.Text = modifier.ToString() + " + " + keyString;
        }

        private void setkey_button_Click(object sender, EventArgs e)
        {
            if (command_list.SelectedItems.Count == 1)
            {
                if (currentKey != Keys.Shift)
                {
                    command_list.SelectedItems[0].SubItems[1].Text = currentModifier + " + " + currentKey;
                    int hotkeyIndex = command_list.SelectedItems[0].Index;
                    settings.GlobalHotkeys[hotkeyIndex].Modifier = currentModifier;
                    settings.GlobalHotkeys[hotkeyIndex].Key = currentKey;
                }
            }
        }
    }
}
