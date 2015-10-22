using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using KoPlayer.PlayLists;
using KoPlayer.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KoPlayer.Forms
{
    public partial class MainForm : Form
    {
        #region Constants
        public const string SETTINGSPATH = @"Settings\settings.xml";
        public const string PLAYLISTDIRECTORYPATH = @"Playlists\";
        public const string PARTYMIXFILEPATH = @"Playlists\Party Mix.xml";
        private const string COLUMNSETTINGSPATH = @"Settings\column_settings.xml";
        private const string DEFAULTCOLUMNSETTINGSPATH = @"Settings\default_column_settings.xml";
        #endregion

        #region Properties
        public Settings Settings { get { return settings; } set { settings = value; } }
        public Random Random { get { return random; } }
        public Song CurrentlyPlaying { get { return this.currentlyPlaying; } }
        #endregion

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
        private PlayList renamePlaylist;
        private int clickedIndex;
        private Song songToSave;
        private KeyboardHook hook;
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

            SetUpGlobalHotkeys();

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

            UpdateShowingPlayList(true);
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
        private void UpdateShowingPlayList(bool getAll)
        {
            if (showingPlayList != null)
            {
                if (getAll)
                    songGridView.DataSource = showingPlayList.GetAllSongs();
                else
                    songGridView.DataSource = showingPlayList.GetSongs();
                if (showingPlayList == partyMix)
                    SetPartyMixColors();
            }
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
                        songGridView.Invoke(new MethodInvoker(delegate { UpdateShowingPlayList(true); }));
                    else
                        UpdateShowingPlayList(true);
                    songGridView.ClearSelection();
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
            settingsWindow.StartPosition = FormStartPosition.CenterParent;
            settingsWindow.ShowDialog();
            this.settings.Save(SETTINGSPATH);
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
            settings.StartupPlayList = library.Name;
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
                    UpdateShowingPlayList(true);
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
                library.LibraryChanged += library_LibraryChanged;
            }
        }

        void library_LibraryChanged(object sender, LibraryChangedEventArgs e)
        {
            if (showingPlayList == library)
                UpdateShowingPlayList(true);
            PopulatePartyMix();
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
                albumArtBox.Image = System.Drawing.Image.FromStream(ms);
            }
            else
                albumArtBox.Image = null;
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
            bool play = true;
            try
            {
                song.Reload();
            }
            catch (SongReloadException ex)
            {
                play = false;
                MessageBox.Show(ex.ToString() + "\nSong will be removed from the library.");
                library.Remove(song);
            }

            if (play)
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

                if (songToSave != null)
                    if (songToSave.Path != currentlyPlaying.Path)
                    {
                        songToSave.SaveTags();
                        songToSave = null;
                    }
            }
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

        #region Local Hotkeys
        private void songGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (songGridView.Focused)
            {
                if (e.Control)
                {
                    if (settings.RatingHotkeys.Contains(e.KeyCode))
                        RateSongs(songGridView.SelectedRows, Array.IndexOf(settings.RatingHotkeys, e.KeyCode));
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    PlaySong(songGridView.SelectedRows[0].DataBoundItem as Song, showingPlayList);
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    DeleteSongs(songGridView.SelectedRows);
                }
            }
        }

        private void RateSongs(DataGridViewSelectedRowCollection rows, int rating)
        {
            foreach (DataGridViewRow row in rows)
            {
                Song s = row.DataBoundItem as Song;
                s.Rating = rating;
            }
            songGridView.Refresh();
            showingPlayList.Save();
        }

        private void DeleteSongs(DataGridViewSelectedRowCollection rows)
        {
            bool shouldDelete = true;
            if (rows.Count > 25)
            {
                string queryMessage = "You are about to delete " + rows.Count + " songs from the ";
                if (showingPlayList != library)
                    queryMessage += "playlist";
                else
                    queryMessage += "library";
                queryMessage += ".\nAre you sure?";

                DialogResult dialogResult = MessageBox.Show(queryMessage, "", MessageBoxButtons.OKCancel);
                if (dialogResult == DialogResult.Cancel)
                    shouldDelete = false;
            }
            if (shouldDelete)
            {
                if (songGridView.AreAllCellsSelected(false))
                    showingPlayList.RemoveAll();
                else
                {
                    List<int> indexList = GetSortedRowIndexes(rows);
                    foreach (int i in indexList)
                    {
                        if (currentlyPlaying != null)
                        {
                            if (showingPlayList == library)
                            {
                                if (currentlyPlaying == (Song)songGridView.Rows[i].DataBoundItem)
                                {
                                    StopPlaying();
                                    albumArtBox.Image = null;
                                }
                            }
                            else
                                if (playingPlayList.CurrentIndex == i)
                                {
                                    StopPlaying();
                                    albumArtBox.Image = null;
                                }
                        }
                    }
                    showingPlayList.Remove(indexList);
                }

                if (showingPlayList == partyMix)
                    PopulatePartyMix();
                showingPlayList.Save();
                UpdateShowingPlayList(false);
            }
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

        #region Global hotkeys

        private void SetUpGlobalHotkeys()
        {
            hook = new KeyboardHook();
            hook.KeyPressed += hook_KeyPressed;
            try
            {
                foreach (GlobalHotkey hk in settings.GlobalHotkeys)
                    hook.RegisterHotKey(hk.Modifier, hk.Key);
            }
            catch
            {
                hook.Dispose();
                hook = new KeyboardHook();
                hook.KeyPressed += hook_KeyPressed;
                MessageBox.Show("Unable to set global hotkeys.\nTry closing other applications that use them and restart.");
            }
        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            foreach (GlobalHotkey hk in settings.GlobalHotkeys)
            {
                if (e.Modifier == hk.Modifier)
                    if (e.Key == hk.Key)
                        switch (hk.Action)
                        {
                            case GlobalHotkeyAction.IncreaseVolume:
                                IncreaseVolume();
                                break;
                            case GlobalHotkeyAction.DecreaseVolume:
                                DecreaseVolume();
                                break;
                            case GlobalHotkeyAction.PlayOrPause:
                                PlayOrPause();
                                break;
                            case GlobalHotkeyAction.ShowSongInfoPopup:
                                ShowCurrentSongPopup();
                                break;
                            case GlobalHotkeyAction.PlayPreviousSong:
                                PlayPreviousSong();
                                break;
                            case GlobalHotkeyAction.PlayNextSong:
                                PlayNextSong();
                                break;
                        }
            }
        }

        private void IncreaseVolume()
        {
            SetVolumeBarValue(volumeTrackBar.Value + volumeTrackBar.SmallChange);
        }

        private void DecreaseVolume()
        {
            SetVolumeBarValue(volumeTrackBar.Value - volumeTrackBar.SmallChange);
        }

        private void ShowCurrentSongPopup()
        {
            //Do popup stuff
        }

        #endregion

        #region Playlist manipulatiom methods
        private void playListGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (playListGridView.Focused)
                if (e.KeyCode == Keys.Delete)
                {
                    DeletePlayList(playListGridView.SelectedCells[0]);
                }
        }

        private void DeletePlayList(DataGridViewCell cell)
        {
            bool deleted = false;
            IPlayList pl = playLists[cell.RowIndex];
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
            
            if (deleted)
                SetPlayListGridView();
        }

        private void RenamePlayList()
        {
            this.renamePlaylist = GetPlayList(playListGridView.CurrentCell.Value.ToString()) as PlayList;
            playListGridView.BeginEdit(true);
        }

        private void playListGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string currentName = playListGridView.CurrentCell.Value.ToString();
            string oldName = this.renamePlaylist.Name;
            string oldPath = this.renamePlaylist.Path;
            bool acceptable = true;
            foreach (IPlayList pl in playLists)
            {
                if (currentName.ToLower() == pl.Name.ToLower())
                    if (currentName.ToLower() != oldName.ToLower())
                        acceptable = false;
            }
            if (acceptable)
            {
                if (currentName.ToLower() != oldName.ToLower())
                {
                    renamePlaylist.Name = currentName;
                    try
                    {
                        System.IO.File.Move(oldPath, renamePlaylist.Path);
                    }
                    catch { }
                }
            }
            else
                playListGridView.BeginEdit(true);
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
                if (playList != library)
                    ResetSearchBox();
                showingPlayList.Save();
                showingPlayList = playList;
                songGridView.ClearSelection();
                playListGridView.Rows[playLists.IndexOf(playList)].Cells[0].Selected = true;
                SetSortGlyph();
                if (showingPlayList == partyMix)
                    PopulatePartyMix();
                else
                    UpdateShowingPlayList(true);
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
                if (value > volumeTrackBar.Maximum)
                    volumeTrackBar.Value = volumeTrackBar.Maximum;
                else if (value < volumeTrackBar.Minimum)
                    volumeTrackBar.Value = volumeTrackBar.Minimum;
                else
                    volumeTrackBar.Value = value;
            }
        }
        #endregion
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
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (showingPlayList != partyMix)
                {
                    showingPlayList.Sort(e.ColumnIndex, songGridView.Columns[e.ColumnIndex].HeaderText);
                    UpdateShowingPlayList(false);
                    SetSortGlyph();
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
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

        private void SetSortGlyph()
        {
            foreach (DataGridViewColumn column in songGridView.Columns)
                column.HeaderCell.SortGlyphDirection = SortOrder.None;
            if (showingPlayList.SortColumnIndex >= 0)
                songGridView.Columns[showingPlayList.SortColumnIndex].HeaderCell.SortGlyphDirection = showingPlayList.SortOrder;
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
            songGridView.DataSource = newPlayList.GetSongs();
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
                            songGridView.DataSource = pl.GetSongs();
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
                            UpdateShowingPlayList(true);
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
                UpdateShowingPlayList(true);
                ResetSearchBox();
            }
        }

        private void ResetSearchBox()
        {
            if (songGridView.InvokeRequired)
                songGridView.Invoke(new MethodInvoker(delegate
                    {
                        searchBox.Text = searchBoxDefault;
                        searchBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                    }));
            else
            {
                searchBox.Text = searchBoxDefault;
                searchBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            }
        }


        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            if (searchBox.Text != searchBoxDefault)
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
            ChangeToPlayList(library);
            if (searchBox.Text.Length > 0)
            {
                songGridView.DataSource = library.Search(searchBox.Text);
                UpdateShowingPlayList(false);
            }
            else if (searchBox.Text.Length == 0)
                UpdateShowingPlayList(true);
            SetSortGlyph();
            
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
            {
                songGridView.Focus();
                if (e.KeyCode == Keys.Escape)
                {
                    ResetSearchBox();
                    UpdateShowingPlayList(true);
                }
                SetSortGlyph();
            }
        }
        #endregion

        #region Right click menus
        private MenuItem CreateMenuItem(string name, System.EventHandler clickEvent)
        {
            MenuItem item = new MenuItem(name);
            item.Click += clickEvent;
            return item;
        }

        #region Playlist gridview right click menu
        private void playListGridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ContextMenu cm = new ContextMenu();
                cm.MenuItems.Add(CreateMenuItem("Create new playlist", newPlaylistToolStripMenuItem_Click));

                int rowIndex = playListGridView.HitTest(e.X, e.Y).RowIndex;

                if (rowIndex >= 0)
                {
                    playListGridView.ClearSelection();
                    playListGridView.Rows[rowIndex].Selected = true;
                    playListGridView.Rows[rowIndex].Cells[0].Selected = true;

                    if (rowIndex > 1)
                    {
                        cm.MenuItems.Add(CreateMenuItem("Delete playlist", playListGridViewRightClickMenuDelete));
                        cm.MenuItems.Add(CreateMenuItem("Rename playlist", playListGridViewRightClickRename));
                    }
                }
                cm.Show(playListGridView, new Point(e.X, e.Y));
            }
        }

        private void playListGridViewRightClickMenuDelete(object sender, EventArgs e)
        {
            DeletePlayList(playListGridView.SelectedCells[0]);
        }

        private void playListGridViewRightClickRename(object sender, EventArgs e)
        {
            RenamePlayList();
        }
        #endregion

        #region Song gridview right click menu

        private void songGridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                this.clickedIndex = songGridView.HitTest(e.X, e.Y).RowIndex;
                if (this.clickedIndex >= 0)
                {
                    if (songGridView.Rows[this.clickedIndex].Cells[0].Selected != true)
                        songGridView.ClearSelection();
                    songGridView.Rows[this.clickedIndex].Cells[0].Selected = true;

                    ContextMenu cm = CreateSongRightClickMen();
                    cm.Show(this, this.PointToClient(Cursor.Position));
                }
            }
        }

        private ContextMenu CreateSongRightClickMen()
        {
            ContextMenu cm = new ContextMenu();
            cm.MenuItems.Add(CreateMenuItem("Play Song", songGridViewRightClickPlay));
            if (musicPlayer.PlaybackState == PlaybackState.Playing)
                cm.MenuItems.Add(CreateMenuItem("Pause", songGridViewRightClickPause));
            if (musicPlayer.PlaybackState == PlaybackState.Paused)
                cm.MenuItems.Add(CreateMenuItem("Resume", songGridViewRightClickResume));
            cm.MenuItems.Add(CreateMenuItem("Queue next in party mix", songGridViewRightClickAddToPartyMixNext));
            cm.MenuItems.Add(CreateMenuItem("Add songs to party mix", songGridViewRightClickAddToPartyMixBottom));
            MenuItem ratingMenu = new MenuItem("Set Rating");
            #region Rating menu
            ratingMenu.MenuItems.Add("Rate 0 (ctrl + §)");
            ratingMenu.MenuItems.Add("Rate 1 (ctrl + 1)");
            ratingMenu.MenuItems.Add("Rate 2 (ctrl + 2)");
            ratingMenu.MenuItems.Add("Rate 3 (ctrl + 3)");
            ratingMenu.MenuItems.Add("Rate 4 (ctrl + 4)");
            ratingMenu.MenuItems.Add("Rate 5 (ctrl + 5)");
            ratingMenu.MenuItems[0].Click += rate0_Click;
            ratingMenu.MenuItems[1].Click += rate1_Click;
            ratingMenu.MenuItems[2].Click += rate2_Click;
            ratingMenu.MenuItems[3].Click += rate3_Click;
            ratingMenu.MenuItems[4].Click += rate4_Click;
            ratingMenu.MenuItems[5].Click += rate5_Click;
            #endregion
            cm.MenuItems.Add(ratingMenu);
            cm.MenuItems.Add(CreateMenuItem("Delete", songGridViewRightClickDelete));
            cm.MenuItems.Add(CreateMenuItem("Properties", songGridViewRightClickProperties));
            return cm;
        }

        #region Right click menu events

        #region Rating menu events
        void rate0_Click(object sender, EventArgs e)
        {
            RateSongs(songGridView.SelectedRows, 0);
        }

        void rate1_Click(object sender, EventArgs e)
        {
            RateSongs(songGridView.SelectedRows, 1);
        }

        void rate2_Click(object sender, EventArgs e)
        {
            RateSongs(songGridView.SelectedRows, 2);
        }

        void rate3_Click(object sender, EventArgs e)
        {
            RateSongs(songGridView.SelectedRows, 3);
        }

        void rate4_Click(object sender, EventArgs e)
        {
            RateSongs(songGridView.SelectedRows, 4);
        }

        void rate5_Click(object sender, EventArgs e)
        {
            RateSongs(songGridView.SelectedRows, 5);
        }
        #endregion

        private void songGridViewRightClickPlay(object sender, EventArgs e)
        {
            PlaySong((Song)songGridView.Rows[clickedIndex].DataBoundItem, showingPlayList);
        }

        private void songGridViewRightClickPause(object sender, EventArgs e)
        {
            if (musicPlayer.PlaybackState == PlaybackState.Playing)
                PauseMusic();
        }

        private void songGridViewRightClickResume(object sender, EventArgs e)
        {
            if (musicPlayer.PlaybackState == PlaybackState.Paused)
                PlayMusic();
        }

        private void songGridViewRightClickAddToPartyMixNext(object sender, EventArgs e)
        {
            List<Song> songs = new List<Song>();
            foreach (DataGridViewRow row in songGridView.SelectedRows)
                songs.Add((Song)row.DataBoundItem);
            partyMix.Insert(partyMix.CurrentIndex + 1, songs);

            if (showingPlayList == partyMix)
                UpdateShowingPlayList(true);
        }

        private void songGridViewRightClickAddToPartyMixBottom(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in songGridView.SelectedRows)
                partyMix.Add((Song)row.DataBoundItem);

            if (showingPlayList == partyMix)
                UpdateShowingPlayList(true);
        }

        private void songGridViewRightClickDelete(object sender, EventArgs e)
        {
            DeleteSongs(songGridView.SelectedRows);
        }

        private void songGridViewRightClickProperties(object sender, EventArgs e)
        {
            Song clickedSong = songGridView.Rows[clickedIndex].DataBoundItem as Song;
            bool exists = true;
            try
            {
                clickedSong.Reload();
            }
            catch (SongReloadException ex)
            {
                exists = false;
                MessageBox.Show(ex.ToString() + "\nSong will be removed from the library.");
                library.Remove(clickedSong);
            }
            if (exists)
            {
                SongInfoPopup popUp = new SongInfoPopup(this, clickedSong, this.clickedIndex, this.showingPlayList);
                popUp.SavePlayingSong += popUp_SavePlayingSong;
                popUp.StartPosition = FormStartPosition.CenterParent;
                popUp.ShowDialog();
            }
            songGridView.Refresh();
        }

        private void popUp_SavePlayingSong(object sender, SavePlayingSongEventArgs e)
        {
            songToSave = e.savingSong;
        }
        #endregion
        #endregion
        #endregion
    }
}
