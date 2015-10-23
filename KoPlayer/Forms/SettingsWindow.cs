using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.Playlists;
using KoPlayer.Forms;
using KoPlayer.Forms.SettingsControls;

namespace KoPlayer.Forms
{
    public partial class SettingsWindow : Form
    {
        private Settings settings;
        private MainForm callingForm;
        private Control showingSettings;
        private List<IPlaylist> playlists;

        public SettingsWindow(MainForm callingForm, List<IPlaylist> playlists)
        {
            this.callingForm = callingForm;
            this.playlists = playlists;
            this.settings = callingForm.Settings;
            InitializeComponent();
        }

        private void PreferencesWindow_Load(object sender, EventArgs e)
        {
            CreateTreeViewNodes();
        }

        private void CreateTreeViewNodes()
        {
            List<TreeNode> treeNodes = new List<TreeNode>();
            treeNodes.Add(CreateTreeNode("Song List"));
            treeNodes.Add(CreateTreeNode("Party Mix"));
            treeNodes.Add(CreateTreeNode("Hot Keys"));
            treeNodes[treeNodes.Count - 1].Nodes.Add(CreateTreeNode("Global")); ;
            //treeNodes.Add(CreateTreeNode("Last.fm Scrobbler"));
            settingsCategoryView.Nodes.AddRange(treeNodes.ToArray());
            settingsCategoryView.Select();
        }

        private TreeNode CreateTreeNode(string name)
        {
            return new TreeNode(name);
        }

        private void save_Button_Click(object sender, EventArgs e)
        {
            //callingForm.Settings = tempSettings;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void cancel_Button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void settingsCategoryView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (showingSettings != null)
            {
                this.Controls.Remove(showingSettings);
                showingSettings.Hide();
                showingSettings.Enabled = false;
                showingSettings = null;
            }

            switch (e.Node.Text)
            {
                case "Song List":
                    SetAndAddControl(new SongListSettingsControl(this.settings));
                    break;
                case "Party Mix":
                    SetAndAddControl(new PartyMixSettingsControl(this.settings, this.playlists));
                    break;
                case "Hot Keys":
                    SetAndAddControl(new HotkeySettingsControl(this.settings));
                    break;
                case "Global":
                    SetAndAddControl(new GlobalHotkeySettingsControl(this.settings));
                    break;
                case "Last.fm Scrobbler":
                    SetAndAddControl(new LastFMSettingsControl(this.settings));
                    break;
            }
            
            ShowCurrentSettings();
        }

        private void SetAndAddControl(Control control)
        {
            control.Location = new Point(189, 12);
            control.Size = new Size(300, 200);
            this.showingSettings = control;
            this.Controls.Add(this.showingSettings);
        }

        private void ShowCurrentSettings()
        {
            if (showingSettings != null)
            {
                this.showingSettings.Enabled = true;
                this.showingSettings.Show();
            }
        }
    }
}
