using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using KoPlayer.Lib;

namespace KoPlayer.Forms
{
    public partial class MultiSongPropertiesWindow : Form
    {
        public event SavePlayingSongEventHandler SavePlayingSong;
        public event SongChangedEventHandler SongChanged;

        private List<Song> songs;
        private PlaylistBase currentPlaylist;
        private Library library;
        private MainForm mainForm;
        private Dictionary<string, bool> saveProperties;

        public MultiSongPropertiesWindow(MainForm mainForm, List<Song> songs, PlaylistBase currentPlaylist, Library library)
        {
            this.mainForm = mainForm;
            this.songs = songs;
            this.currentPlaylist = currentPlaylist;
            this.library = library;
            this.saveProperties = new Dictionary<string, bool>();
            InitializeComponent();

            save_ComboBox.SelectedIndex = 0;
            for (int i = 2; i < save_ComboBox.Items.Count; i++)
                this.saveProperties.Add(save_ComboBox.Items[i].ToString(), false);
        }

        private void SongInfoPopup_Load(object sender, EventArgs e)
        {
            // If a certain property has the same value for all songs, set that textbox text to the value
            foreach (Control c in this.Controls)
            {
                if (c.Tag != null)
                    if (PropertySame(c.Tag.ToString()))
                    {
                        c.Text = this.songs[0][c.Tag.ToString()];
                        if (c.Text == DateTime.MinValue.ToShortDateString())
                            c.Text = "";
                    }
            }
            SetAllProperties(false);
        }

        private bool PropertySame(string property)
        {
            string comparer = this.songs[0][property];
            foreach (Song s in this.songs)
                if (s[property] != comparer)
                    return false;
            return true;
        }

        #region Button events
        private void ok_button_Click(object sender, EventArgs e)
        {
            if (saveProperties.Values.Contains(true))
            {
                string queryMessage = "You are about change the tags of " + songs.Count + " songs."
                    + "\nAre you sure about this?";

                DialogResult dialogResult = MessageBox.Show(queryMessage, "", MessageBoxButtons.OKCancel);

                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    foreach (Song s in songs)
                        SaveSong(s);
                    this.Close();
                }
            }
            else
                MessageBox.Show("No property is marked for saving.");
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
        #endregion

        private void SaveSong(Song song)
        {
            if (this.saveProperties["Title"])
                song.Title = this.title_box.Text;
            if (this.saveProperties["Artist"])
                song.Artist = this.artist_box.Text;
            if (this.saveProperties["Album"])
                song.Album = this.album_box.Text;
            if (this.saveProperties["Track Number"])
                song.TrackNumber = Convert.ToInt32(this.tracknr_box.Text);
            if (this.saveProperties["Disc Number"])
                song.DiscNumber = Convert.ToInt32(this.discnr_box.Text);
            if (this.saveProperties["Genre"])
                song.Genre = this.genre_box.Text;
            if (this.saveProperties["Rating"])
                song.Rating = (int)this.rating_numupdownstring.Value;

            if (song != this.mainForm.CurrentlyPlaying)
                song.Save();
            else
                OnSavePlayingSong(song);

            OnSongChanged(song);

            this.library.UpdateSongInfo(song);
        }

        private void OnSongChanged(Song song)
        {
            if (SongChanged != null)
                SongChanged(this, new SongChangedEventArgs(song));
        }

        private void OnSavePlayingSong(Song song)
        {
            if (SavePlayingSong != null)
                SavePlayingSong(this, new SavePlayingSongEventArgs(song));
        }

        private void nr_box_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void save_ComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            SetCheckBox();
        }

        private void SetCheckBox()
        {
            string selectedItem = save_ComboBox.SelectedItem.ToString();

            if (selectedItem == "Select Property")
                save_CheckBox.Enabled = false;
            else
            {
                save_CheckBox.Enabled = true;

                if (selectedItem == "All")
                    save_CheckBox.Checked = AllChecked();
                else
                    save_CheckBox.Checked = saveProperties[selectedItem];
            }
        }

        private bool AllChecked()
        {
            foreach (bool value in saveProperties.Values)
                if (value != true)
                    return false;
            return true;
        }

        private void save_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (save_ComboBox.SelectedIndex != 0)
            {
                if (save_ComboBox.SelectedItem.ToString() != "All")
                    saveProperties[save_ComboBox.SelectedItem.ToString()] = save_CheckBox.Checked;
                else
                    SetAllProperties(save_CheckBox.Checked);
            }
        }

        private void SetAllProperties(bool value)
        {
            string[] keys = saveProperties.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
                saveProperties[keys[i]] = value;
        }

        private void box_TextChanged(object sender, EventArgs e)
        {
            Control senderControl = sender as Control;
            saveProperties[senderControl.Tag.ToString()] = true;
            SetCheckBox();
        }
    }
}
