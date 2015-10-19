#region Using statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using KoPlayer.PlayLists;
using System.Threading;
using System.IO;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.Streams;
using KoPlayer;
#endregion

namespace KoPlayer.Forms
{
    public partial class MainForm : Form
    {
        public const string SETTINGSPATH = @"Settings\settings.xml";
        public const string PLAYLISTDIRECTORYPATH = @"Playlists\";
        public const string PARTYMIXFILEPATH = @"Playlists\Party Mix.xml";
        private const string COLUMNSETTINGSPATH = @"Settings\column_settings.xml";
        private const string DEFAULTCOLUMNSETTINGSPATH = @"Settings\default_column_settings.xml";

        public Settings Settings { get { return settings; } set { settings = value; } }
        public Random Random { get { return random; } }

        #region Fields
        private SettingsWindow settingsWindow;
        private Library library;
        private IPlayList showingPlayList;
        private IPlayList playingPlayList;
        private PlayList partyMix;
        private List<IPlayList> playLists;
        private Settings settings;
        private ColumnSettings columnSettings;
        private Song currentlyPlaying;
        private readonly MusicPlayer musicPlayer = new MusicPlayer();
        private MMDevice defaultAudioDevice;
        private System.Timers.Timer searchBarTimer;
        private int searchBarTimerInterval = 100;
        private bool stopSearchBarUpdate = false;
        private delegate void SetSearchBarValueCallback(int value);
        private delegate void SetVolumeBarValueCallback(int value);
        private static Random random = new Random();
        private string searchBoxDefault = "Search Library";
        private System.Timers.Timer searchLibraryTimer;
        private int searchLibraryTimerInterval = 500;
        #endregion

        #region Constructor & Load event
        public MainForm()
        {
            library = Library.Load();
            if (library == null)
            {
                library = new Library();
                library.Save();
            }

            settings = Settings.Load(SETTINGSPATH);
            if (settings == null)
                settings = new Settings();

            var enumerator = new MMDeviceEnumerator();
            defaultAudioDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            this.trayIcon.Icon = ((System.Drawing.Icon)(this.Icon));
            this.Width = settings.FormWidth;
            this.Height = settings.FormHeight;

            ResetSearchBarTimer();
            LoadPlayLists();

            showingPlayList = GetPlayList(settings.StartupPlayList);
            if (showingPlayList == null)
                showingPlayList = library;
            playingPlayList = showingPlayList;

            songGridView.RowTemplate.Height = settings.RowHeight;
            songGridView.DefaultCellStyle.Font = new Font(settings.FontName, settings.FontSize, GraphicsUnit.Point);

            UpdateShowingPlayList();
            songGridView.AutoGenerateColumns = false;

            columnSettings = ColumnSettings.Load(COLUMNSETTINGSPATH);
            if (columnSettings == null)
                columnSettings = ColumnSettings.Load(DEFAULTCOLUMNSETTINGSPATH);
            if (columnSettings == null)
                columnSettings = new ColumnSettings(songGridView.Columns);

            //Sets the columns
            foreach (ColumnSetting cs in columnSettings.SettingList)
            {
                songGridView.Columns[cs.Name].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                songGridView.Columns[cs.Name].DisplayIndex = cs.DisplayIndex;
                songGridView.Columns[cs.Name].Visible = cs.Visible;
                songGridView.Columns[cs.Name].Width = cs.Width;
                songGridView.Columns[cs.Name].SortMode = DataGridViewColumnSortMode.Automatic;
            }

            SetPlayListGridView();
            PopulatePartyMix();
            UpdateTrayIconText();
        }
        #endregion

        #region Playlist methods
        private void UpdateShowingPlayList()
        {
            if (showingPlayList != null)
                songGridView.DataSource = showingPlayList.GetAll();
        }

        private void LoadPlayLists()
        {
            playLists = new List<IPlayList>();
            playLists.Add(library);
            partyMix = PlayList.Load(PARTYMIXFILEPATH, library);
            if (partyMix == null)
            {
                partyMix = new PlayList(library, "Party Mix");
                partyMix.Save();
            }
            playLists.Add(partyMix);
            string[] playListFiles = Directory.GetFiles(PLAYLISTDIRECTORYPATH, "*.xml", SearchOption.AllDirectories);
            foreach (string playListPath in playListFiles)
                if (playListPath != PARTYMIXFILEPATH)
                {
                    PlayList pl = PlayList.Load(playListPath, library);
                    if (pl != null)
                        playLists.Add(pl);
                }
        }

        private void SetPlayListGridView()
        {
            playListGridView.Rows.Clear();
            playListGridView.AllowUserToAddRows = true;
            foreach (IPlayList playList in playLists)
            {
                DataGridViewRow row = (DataGridViewRow)playListGridView.Rows[0].Clone();
                row.Cells[0].Value = playList.Name;
                playListGridView.Rows.Add(row);
            }
            playListGridView.AllowUserToAddRows = false;

            foreach (DataGridViewRow row in playListGridView.Rows)
            {
                if (row.Cells[0].Value.ToString().ToLower() == showingPlayList.Name.ToLower())
                    row.Selected = true;
                else
                    row.Selected = false;
            }
            playListGridView.Update();
        }

        private IPlayList GetPlayList(string name)
        {
            foreach (IPlayList pl in playLists)
                if (pl.Name.ToLower() == name.ToLower())
                    return pl;
            return null;
        }

        #region Party mix
        private void PopulatePartyMix()
        {
            IPlayList source = GetPlayList(settings.Partymix_SourcePlayListName);
            if (source == null)
            {
                source = library;
                settings.Partymix_SourcePlayListName = library.Name;
            }

            if (source.NumSongs > 0)
            {
                while (partyMix.CurrentIndex > settings.Partymix_NumPrevious)
                    partyMix.Remove(0);

                while (partyMix.NumSongs - partyMix.CurrentIndex < settings.Partymix_NumNext + 1)
                {
                    partyMix.Add(source.GetRandom());
                }

                if (showingPlayList == partyMix)
                {
                    if (songGridView.InvokeRequired)
                        songGridView.Invoke(new MethodInvoker(delegate { UpdateShowingPlayList(); }));
                    else
                        UpdateShowingPlayList();
                    songGridView.ClearSelection();
                    SetPartyMixColors();
                }
            }
        }

        private void SetPartyMixColors()
        {
            foreach (DataGridViewRow row in songGridView.Rows)
            {
                if (row.Index == partyMix.CurrentIndex)
                    row.DefaultCellStyle.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
                else if (row.Index % 2 == 0)
                    row.DefaultCellStyle.BackColor = Color.White;
                else
                    row.DefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
            }
        }
        #endregion

        #endregion

        #region Settings window
        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingsWindow = new SettingsWindow(this);
            settingsWindow.FormClosed += settingsWindow_FormClosed;
            this.Enabled = false;
        }

        void settingsWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Enabled = true;
            this.settings.Save(SETTINGSPATH);
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (settingsWindow != null && this.Enabled == false)
                settingsWindow.Focus();
        }
        #endregion

        #region Exit
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.FormWidth = this.Width;
            settings.FormHeight = this.Height;
            settings.StartupPlayList = showingPlayList.Name;
            settings.Save(SETTINGSPATH);

            columnSettings = new ColumnSettings(songGridView.Columns);
            columnSettings.Save(COLUMNSETTINGSPATH);

            foreach (IPlayList pl in playLists)
                pl.Save();

            searchBarTimer.Stop();
            searchBarTimer.Dispose();
            if (searchLibraryTimer != null)
            {
                searchLibraryTimer.Stop();
                searchLibraryTimer.Dispose();
            }
            musicPlayer.Dispose();
            trayIcon.Dispose();
        }
        #endregion

        #region Add songs
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = dialog.FileName;
                library.Add(path);
                if (showingPlayList != library)
                {
                    showingPlayList.Add(path);
                    UpdateShowingPlayList();
                }
            }
        }

        private void addFolderToLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                library.AddFolder(dialog.SelectedPath);
                statusStrip1.Items.Add("Adding files...");
                ToolStripProgressBar progressBar = new ToolStripProgressBar("add_progress");
                progressBar.Minimum = 0;
                progressBar.Maximum = 100;
                statusStrip1.Items.Add(progressBar);
                library.ReportProgress += library_ReportProgress;
            }
        }

        void library_ReportProgress(object sender, ReportProgressEventArgs e)
        {
            if (e.ProgressPercentage < 100)
            {
                ToolStripProgressBar progressBar = (ToolStripProgressBar)statusStrip1.Items["add_progress"];
                progressBar.Value = (int)(e.ProgressPercentage);
            }
            else if (e.ProgressPercentage == 100)
            {
                statusStrip1.Items.Clear();
                library.ReportProgress -= library_ReportProgress;
            }
        }
        #endregion

        #region Control updates
        private void UpdateTrayIconText()
        {
            string text = "";
            if (musicPlayer.PlaybackState != PlaybackState.Stopped && currentlyPlaying != null)
            {
                int limit = 42; // (128 - 1) / 3 = 42.333;
                text += ShortenString(currentlyPlaying.Title, limit) + "\n";
                text += ShortenString(currentlyPlaying.Artist, limit) + "\n";
                text += ShortenString(currentlyPlaying.Album, limit);
            }
            else
                text = this.Text;
            Fixes.SetNotifyIconText(trayIcon, text);
        }

        private string ShortenString(string s, int limit)
        {
            if (s.Length > limit)
                return s.Substring(0, limit - 3) + "..";
            else
                return s;
        }

        private void UpdateSongImage()
        {
            TagLib.File tagFile = TagLib.File.Create(currentlyPlaying.Path);
            if (tagFile.Tag.Pictures.Length > 0)
            {
                MemoryStream ms = new MemoryStream(tagFile.Tag.Pictures[0].Data.Data);
                pictureBox1.Image = System.Drawing.Image.FromStream(ms);
            }
            else
                pictureBox1.Image = null;
        }

        private void UpdateSongInfoLabel()
        {
            if (songInfoLabel.InvokeRequired)
                songInfoLabel.Invoke(new MethodInvoker(delegate { SetSongInfoLabelText(); }));
            else
                SetSongInfoLabelText();
        }

        private void SetSongInfoLabelText()
        {
            if (musicPlayer.PlaybackState != PlaybackState.Stopped)
                songInfoLabel.Text =
                      currentlyPlaying.Title + "\n"
                    + currentlyPlaying.Artist + "\n"
                    + currentlyPlaying.Album;
            else
                songInfoLabel.Text = "";
        }
        #endregion

        #region Playback control methods
        private void PlaySong(Song song, IPlayList inPlayList)
        {
            if (inPlayList == partyMix)
                PopulatePartyMix();

            ResetSearchBarTimer();

            musicPlayer.Volume = 0;
            musicPlayer.Stop();
            searchBarTimer.Stop();

            try
            {
                musicPlayer.Open(song.Path, defaultAudioDevice);
                PlayMusic();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Play music exception: " + ex.ToString());
            }

            currentlyPlaying = song;
            playingPlayList = inPlayList;

            UpdateTrayIconText();
            UpdateSongImage();
            UpdateSongLengthLabel();
            UpdateSongInfoLabel();
        }

        private void PlayMusic()
        {
            musicPlayer.Play();
            if (volumeTrackBar.InvokeRequired)
            {
                volumeTrackBar.Invoke(new MethodInvoker(delegate { musicPlayer.Volume = volumeTrackBar.Value; }));
            }
            else
                musicPlayer.Volume = volumeTrackBar.Value;
            UpdateSearchBar(musicPlayer.Length, musicPlayer.Position);
            searchBarTimer.Start();
        }

        private void PauseMusic()
        {
            musicPlayer.Volume = 0;
            musicPlayer.Pause();
            searchBarTimer.Stop();
        }

        private void StopPlaying()
        {
            musicPlayer.Volume = 0;
            musicPlayer.Stop();
            searchBarTimer.Stop();
            if (songInfoLabel.InvokeRequired)
                songInfoLabel.Invoke(new MethodInvoker(delegate { SetSongInfoLabelText(); }));
            else
                SetSongInfoLabelText();
        }

        private void PlayNextSong()
        {
            Song nextSong = playingPlayList.GetNext();
            if (nextSong != null)
                PlaySong(nextSong, playingPlayList);
        }

        private void PlayPreviousSong()
        {
            Song previousSong = playingPlayList.GetPrevious();
            if (previousSong != null)
                PlaySong(previousSong, playingPlayList);
        }

        private void PlayOrPause()
        {
            if (musicPlayer.PlaybackState == PlaybackState.Paused)
            {
                PlayMusic();
            }
            else if (musicPlayer.PlaybackState == PlaybackState.Playing)
            {
                PauseMusic();
            }
            else if (musicPlayer.PlaybackState == PlaybackState.Stopped)
            {
                if (songGridView.Rows.Count > showingPlayList.CurrentIndex)
                    if (songGridView.Rows[showingPlayList.CurrentIndex].DataBoundItem != null)
                        PlaySong(songGridView.Rows[showingPlayList.CurrentIndex].DataBoundItem as Song, showingPlayList);
            }
        }

        private void songGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                showingPlayList.CurrentIndex = e.RowIndex;
                PlaySong((Song)songGridView.Rows[e.RowIndex].DataBoundItem, showingPlayList);
            }
        }
        #endregion

        #region Delete Songs
        private void songGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (songGridView.Focused)
                if (e.KeyCode == Keys.Delete)
                {
                    DeleteSongs(songGridView.SelectedRows);
                }
        }

        private void DeleteSongs(DataGridViewSelectedRowCollection rows)
        {
            List<int> indexList = GetSortedRowIndexes(rows);
            foreach (int i in indexList)
            {
                if (currentlyPlaying != null)
                    if (playingPlayList.CurrentIndex == i)
                        StopPlaying();
                showingPlayList.Remove(i);
            }
            
            if (showingPlayList == partyMix)
                PopulatePartyMix();
            showingPlayList.Save();
            UpdateShowingPlayList();
        }

        private List<int> GetSortedRowIndexes(DataGridViewSelectedRowCollection rows)
        {
            List<int> indexList = new List<int>();
            foreach (DataGridViewRow row in rows)
                indexList.Add(row.Index);
            indexList.Sort();
            indexList.Reverse();
            return indexList;
        }
        #endregion

        #region Delete Playlist
        private void playListGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (playListGridView.Focused)
                if (e.KeyCode == Keys.Delete)
                {
                    DeletePlayList(playListGridView.SelectedRows);
                }
        }

        private void DeletePlayList(DataGridViewSelectedRowCollection rows)
        {
            bool deleted = false;
            foreach (DataGridViewRow row in rows)
            {
                IPlayList pl = playLists[row.Index];
                if (pl != library && pl != partyMix)
                {
                    if (pl == showingPlayList)
                        ChangeToPlayList(library);
                    if (pl == playingPlayList)
                    {
                        StopPlaying();
                        playingPlayList = library;
                    }
                    playLists.Remove(pl);
                    try
                    {
                        File.Delete(pl.Path);
                    }
                    catch { }
                    deleted = true;
                }
            }
            if (deleted)
                SetPlayListGridView();
        }
        #endregion

        #region Playlist selection
        private void playListGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ChangeToPlayList(playLists[e.RowIndex]);
        }

        private void ChangeToPlayList(IPlayList playList)
        {
            if (showingPlayList != playList)
            {
                ResetSearchBox();
                showingPlayList.Save();
                showingPlayList = playList;
                UpdateShowingPlayList();
                songGridView.ClearSelection();
                if (showingPlayList == partyMix)
                    PopulatePartyMix();
            }
        }
        #endregion

        #region Playback timer methods
        private void ResetSearchBarTimer()
        {
            if (searchBarTimer != null)
                searchBarTimer.Dispose();
            searchBarTimer = new System.Timers.Timer(searchBarTimerInterval);
            searchBarTimer.Elapsed += searchBarTimer_Elapsed;
            SetSearchBarValue(0);
        }

        private void searchBarTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan length = musicPlayer.Length;
            TimeSpan position = musicPlayer.Position;

            UpdateSearchBar(length, position);
            UpdateCurrentTimeLabel(length, position);

            if ((searchBarTimer.Enabled && musicPlayer.PlaybackState == PlaybackState.Stopped)
                || searchBarTimer.Enabled && position > length)
                PlayNextSong();
        }
        #endregion

        #region Updates for controls that change during playback
        private void UpdateSearchBar(TimeSpan length, TimeSpan position)
        {
            if (position > length)
                length = position;

            if (!stopSearchBarUpdate && length != TimeSpan.Zero && position != TimeSpan.Zero)
            {
                double perc = position.TotalMilliseconds / length.TotalMilliseconds * searchBar.Maximum;
                SetSearchBarValue((int)perc);
            }
        }

        private void UpdateCurrentTimeLabel(TimeSpan length, TimeSpan position)
        {
            if (position > length)
                position = length;

            if (currentTime_Label.InvokeRequired && !currentTime_Label.IsDisposed)
                currentTime_Label.Invoke(new MethodInvoker(
                    delegate 
                    {
                        currentTime_Label.Text = Song.DurationFromTimeSpanToString(musicPlayer.Position);
                    }));
            else
                currentTime_Label.Text = Song.DurationFromTimeSpanToString(musicPlayer.Position);
        }

        private void UpdateSongLengthLabel()
        {
            if (songInfoLabel.InvokeRequired)
                songInfoLabel.Invoke(new MethodInvoker(delegate
                    {
                        songLength_Label.Text = Song.DurationFromTimeSpanToString(musicPlayer.Length);
                    }));
            else
                songLength_Label.Text = Song.DurationFromTimeSpanToString(musicPlayer.Length);
        }
        #endregion

        #region Set control value methods
        private void SetSearchBarValue(int value)
        {
            if (this.searchBar.InvokeRequired)
            {
                try
                {
                    SetSearchBarValueCallback d = new SetSearchBarValueCallback(SetSearchBarValue);
                    this.Invoke(d, new object[] { value });
                }
                catch { }
            }
            else
            {
                searchBar.Value = value;
            }
        }

        private void SetVolumeBarValue(int value)
        {
            if (this.volumeTrackBar.InvokeRequired)
            {
                try
                {
                    SetSearchBarValueCallback d = new SetSearchBarValueCallback(SetVolumeBarValue);
                    this.Invoke(d, new object[] { value });
                }
                catch { }
            }
            else
            {
                volumeTrackBar.Value = value;
            }
        }
        #endregion

        #region Searchbar events
        private void searchBar_ValueChanged(object sender, EventArgs e)
        {
            if (stopSearchBarUpdate)
            {
                double perc = searchBar.Value / (double)searchBar.Maximum;
                TimeSpan position = TimeSpan.FromMilliseconds(musicPlayer.Length.TotalMilliseconds * perc);
                musicPlayer.Position = position;
            }
        }

        private void searchBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //Stupid things because scrollbar.Width includes empty space at sides
                double mousePos = e.X;
                double width = (double)searchBar.Width;
                if (mousePos / searchBar.Width > 0.4)
                    width *= 0.98;
                else if (mousePos / searchBar.Width < 0.6)
                    mousePos -= 5;
                int value = (int)((mousePos / width) * searchBar.Maximum);
                if (value > searchBar.Maximum)
                    value = searchBar.Maximum;
                if (value < searchBar.Minimum)
                    value = searchBar.Minimum;
                SetSearchBarValue(value);
                double perc = searchBar.Value / (double)searchBar.Maximum;
                TimeSpan position = TimeSpan.FromMilliseconds(musicPlayer.Length.TotalMilliseconds * perc);
                musicPlayer.Position = position;
                stopSearchBarUpdate = true;
            }
        }

        private void searchBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                stopSearchBarUpdate = false;
        }

        private void searchBar_MouseWheel(object sender, EventArgs e)
        {
            songGridView.Focus();
            HandledMouseEventArgs ee = (HandledMouseEventArgs)e;
            ee.Handled = true;
        }
        #endregion

        #region Button events
        private void nextButton_Click(object sender, EventArgs e)
        {
            PlayNextSong();
        }

        private void playpauseButton_Click(object sender, EventArgs e)
        {
            PlayOrPause();
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            PlayPreviousSong();
        }
        #endregion

        #region Column header events
        private void songGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ContextMenu cm = new ContextMenu();
                foreach (DataGridViewColumn column in songGridView.Columns)
                {
                    MenuItem item = new MenuItem(column.HeaderText);
                    item.Name = column.Name;
                    item.Checked = column.Visible;
                    item.Click += columnHeaderContextMenu_Click;
                    cm.MenuItems.Add(item);
                }
                cm.Show(this, this.PointToClient(Cursor.Position));
            }
        }

        private void columnHeaderContextMenu_Click(object sender, EventArgs e)
        {
            MenuItem senderItem = sender as MenuItem;
            songGridView.Columns[senderItem.Name].Visible = !songGridView.Columns[senderItem.Name].Visible;
            columnSettings = new ColumnSettings(songGridView.Columns);
        }
        #endregion

        #region Volume trackbar
        private void volumeTrackBar_ValueChanged(object sender, EventArgs e)
        {
            musicPlayer.Volume = volumeTrackBar.Value;
        }

        private void volumeTrackBar_MouseWheel(object sender, EventArgs e)
        {
            songGridView.Focus();
            HandledMouseEventArgs ee = (HandledMouseEventArgs)e;
            ee.Handled = true;
        }
        #endregion

        #region New playlist
        private void newPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewPlayList();
        }

        private void CreateNewPlayList()
        {
            string name = "";
            int i = 0;
            bool taken;
            do
            {
                taken = false;
                i++;
                name = "New playlist " + i;
                foreach (IPlayList pl in playLists)
                {
                    if (pl.Name.ToLower() == name.ToLower())
                        taken = true;
                }
            }
            while (taken);

            IPlayList newPlayList = new PlayList(library, name);
            playLists.Add(newPlayList);
            SetPlayListGridView();
            songGridView.DataSource = newPlayList.GetAll();
        }
        #endregion

        #region Drag & Drop songs
        private void songGridView_RowDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                songGridView.DoDragDrop(e.Item, DragDropEffects.All);
            }
        }

        private void songGridView_DragEnter(object sender, DragEventArgs e)
        {
            if (showingPlayList != library)
                e.Effect = DragDropEffects.Move;
        }

        private void songGridView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DataGridViewSelectedRowCollection)))
            {
                Point p = songGridView.PointToClient(new Point(e.X, e.Y));
                DataGridView.HitTestInfo info = songGridView.HitTest(p.X, p.Y);
                if (info.RowIndex >= 0)
                {
                    DataGridViewSelectedRowCollection rows;
                    rows = (DataGridViewSelectedRowCollection)e.Data.GetData(typeof(DataGridViewSelectedRowCollection));

                    IPlayList pl = showingPlayList;
                    if (pl != library)
                    {
                        List<int> indexes = GetSortedRowIndexes(rows);
                        foreach (int index in indexes)
                            pl.Remove(index);
                        foreach (DataGridViewRow row in rows)
                        {
                            Song s = (Song)row.DataBoundItem;
                            PlayList playList = pl as PlayList;
                            playList.Insert(info.RowIndex, s);
                        }

                        if (showingPlayList == partyMix)
                            PopulatePartyMix();
                        else
                            songGridView.DataSource = pl.GetAll();
                    }
                }
            }
        }

        private void playListGridView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void playListGridView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DataGridViewSelectedRowCollection)))
            {
                Point p = playListGridView.PointToClient(new Point(e.X, e.Y));
                DataGridView.HitTestInfo info = playListGridView.HitTest(p.X, p.Y);
                if (info.RowIndex >= 0)
                {
                    DataGridViewSelectedRowCollection rows;
                    rows = (DataGridViewSelectedRowCollection)e.Data.GetData(typeof(DataGridViewSelectedRowCollection));

                    IPlayList pl = GetPlayList(playListGridView.Rows[info.RowIndex].Cells[0].Value.ToString());
                    if (pl != library)
                    {
                        foreach (DataGridViewRow row in rows)
                        {
                            Song s = (Song)row.DataBoundItem;
                            pl.Add(s.Path);
                        }
                        if (pl == showingPlayList)
                            UpdateShowingPlayList();
                    }
                }
            }
        }
        #endregion

        #region Minimize to tray
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            //Updates stops flickering when window de-minimizes
            this.nextButton.Update();
            this.playpauseButton.Update();
            this.previousButton.Update();
            this.songInfoLabel.Update();
            this.playListGridView.Update();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                trayIcon.Visible = true;
                this.Hide();
            }
        }
        #endregion

        #region Search
        private void searchBox_Enter(object sender, EventArgs e)
        {
            if (searchBox.Text == searchBoxDefault)
                searchBox.Clear();
            searchBox.ForeColor = Color.Black;
        }

        private void searchBox_Leave(object sender, EventArgs e)
        {
            if (searchBox.Text.Length == 0)
            {
                UpdateShowingPlayList();
                ResetSearchBox();
            }
        }

        private void ResetSearchBox()
        {
            searchBox.Text = searchBoxDefault;
            searchBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
        }


        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            if (searchBox.Text != searchBoxDefault && searchBox.Text.Length > 0)
            {
                if (searchLibraryTimer != null)
                {
                    searchLibraryTimer.Stop();
                    searchLibraryTimer.Dispose();
                }
                searchLibraryTimer = new System.Timers.Timer(searchLibraryTimerInterval);
                searchLibraryTimer.AutoReset = false;
                searchLibraryTimer.Elapsed += searchLibraryTimer_Elapsed;
                searchLibraryTimer.Start();
            }
        }

        void searchLibraryTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            searchLibraryTimer.Stop();

            ChangeToPlayList(library);
            if (searchBox.Text != searchBoxDefault)
                if (searchBox.Text.Length > 0 || searchBox.Text.Length == 0)
                {
                    if (songGridView.InvokeRequired)
                        songGridView.Invoke(new MethodInvoker(delegate { DoSearch(); }));
                    else
                        DoSearch();
                }
        }

        private void DoSearch()
        {
            if (searchBox.Text.Length > 0)
                songGridView.DataSource = library.Search(searchBox.Text);
            else if (searchBox.Text.Length == 0)
                UpdateShowingPlayList();
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
            {
                songGridView.Focus();
                if (e.KeyCode == Keys.Escape)
                {
                    ResetSearchBox();
                    UpdateShowingPlayList();
                }
            }
        }
        #endregion
    }
}
