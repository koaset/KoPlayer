using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using KoPlayer.Playlists;
using KoPlayer.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public const string SHUFFLEQUEUEFILEPATH = @"Playlists\Shuffle Queue.pl";
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
        private IPlaylist showingPlaylist;
        private IPlaylist playingPlaylist;
        private Playlist shuffleQueue;
        private List<IPlaylist> playlists;
        private Settings settings;
        private ColumnSettings columnSettings;
        private Song currentlyPlaying;
        private readonly MusicPlayer musicPlayer = new MusicPlayer();
        private MMDevice defaultAudioDevice;
        private System.Timers.Timer searchBarTimer;
        private int searchBarTimerInterval = 100;
        private bool stopSearchBarUpdate = false;
        private static Random random = new Random();
        private string searchBoxDefault = "Search Library";
        private System.Timers.Timer searchLibraryTimer;
        private int searchLibraryTimerInterval = 500;
        private Playlist renamePlaylist;
        private int clickedSongIndex;
        private int clickedPlaylistIndex;
        private Song songToSave;
        private KeyboardHook hook;
        private TimeSpan oldPosition;
        private TimeSpan currentSongTimePlayed;
        private int songInfoPopupTime = 3000;
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
            LoadPlaylists();

            showingPlaylist = GetPlaylist(settings.StartupPlaylist);
            if (showingPlaylist == null)
                showingPlaylist = library;
            playingPlaylist = showingPlaylist;

            SetSongGridViewStyle();

            UpdateShowingPlaylist(true);
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

            SetPlaylistGridView();
            PopulateShuffleQueue();
            UpdateTrayIconText();
        }

        private void SetSongGridViewStyle()
        {
            songGridView.RowTemplate.Height = settings.RowHeight;
            songGridView.DefaultCellStyle.Font = new Font(settings.FontName, settings.FontSize, GraphicsUnit.Point);
        }
        #endregion

        #region Playlist methods
        private void UpdateShowingPlaylist(bool getAll)
        {
            if (showingPlaylist != null)
            {
                if (getAll)
                {
                    songGridView.DataSource = showingPlaylist.GetAllSongs();
                }
                else
                    songGridView.DataSource = showingPlaylist.GetSongs();
                if (showingPlaylist == shuffleQueue)
                    SetShuffleQueueColors();
            }
        }

        private void LoadPlaylists()
        {
            playlists = new List<IPlaylist>();
            playlists.Add(library);

            shuffleQueue = Playlist.Load(SHUFFLEQUEUEFILEPATH, library);
            if (shuffleQueue == null)
            {
                shuffleQueue = new Playlist(library, "Shuffle Queue");
                shuffleQueue.Save();
            }
            playlists.Add(shuffleQueue);

            string[] playlistFiles = Directory.GetFiles(PLAYLISTDIRECTORYPATH, "*.pl", SearchOption.AllDirectories);
            foreach (string playlistPath in playlistFiles)
                if (playlistPath != SHUFFLEQUEUEFILEPATH)
                {
                    Playlist pl = Playlist.Load(playlistPath, library);
                    if (pl != null)
                        playlists.Add(pl);
                }

            playlistFiles = Directory.GetFiles(PLAYLISTDIRECTORYPATH, "*.fpl", SearchOption.AllDirectories);
            foreach (string playlistPath in playlistFiles)
            {
                RatingFilterPlaylist pl = RatingFilterPlaylist.Load(playlistPath, library);
                if (pl != null)
                    playlists.Add(pl);
            }
        }

        private void SetPlaylistGridView()
        {
            playlistGridView.Rows.Clear();
            playlistGridView.AllowUserToAddRows = true;
            foreach (IPlaylist playlist in playlists)
            {
                DataGridViewRow row = (DataGridViewRow)playlistGridView.Rows[0].Clone();
                row.Cells[0].Value = playlist.Name;
                playlistGridView.Rows.Add(row);
            }
            playlistGridView.AllowUserToAddRows = false;

            foreach (DataGridViewRow row in playlistGridView.Rows)
            {
                if (row.Cells[0].Value.ToString().ToLower() == showingPlaylist.Name.ToLower())
                    row.Selected = true;
                else
                    row.Selected = false;
            }
            playlistGridView.Update();
        }

        private IPlaylist GetPlaylist(string name)
        {
            foreach (IPlaylist pl in playlists)
                if (pl.Name.ToLower() == name.ToLower())
                    return pl;
            return null;
        }

        #region Shuffle Queue
        private void PopulateShuffleQueue()
        {
            IPlaylist source = GetPlaylist(settings.Shufflequeue_SourcePlaylistName);
            if (source == null)
            {
                source = library;
                settings.Shufflequeue_SourcePlaylistName = library.Name;
            }

            while (shuffleQueue.CurrentIndex > settings.Shufflequeue_NumPrevious)
                shuffleQueue.Remove(0);

            if (source.NumSongs > 0)
                while (shuffleQueue.NumSongs - shuffleQueue.CurrentIndex < settings.Shufflequeue_NumNext + 1)
                    shuffleQueue.Add(source.GetRandom());

            if (showingPlaylist == shuffleQueue)
            {
                if (songGridView.InvokeRequired)
                    songGridView.Invoke(new MethodInvoker(delegate { UpdateShowingPlaylist(true); }));
                else
                    UpdateShowingPlaylist(true);
                songGridView.ClearSelection();
            }
        }

        private void SetShuffleQueueColors()
        {
            foreach (DataGridViewRow row in songGridView.Rows)
            {
                if (row.Index == shuffleQueue.CurrentIndex)
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
            hook.Dispose();
            settingsWindow = new SettingsWindow(this, this.playlists);
            settingsWindow.StartPosition = FormStartPosition.CenterParent;
            this.settings.Save(SETTINGSPATH);
            
            if (settingsWindow.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SetSettings();
            else
                this.settings = Settings.Load(SETTINGSPATH);
            SetUpGlobalHotkeys();
        }

        private void SetSettings()
        {
            SetSongGridViewStyle();

            PopulateShuffleQueue();

            //last.fm

            UpdateShowingPlaylist(true);
            ResetSearchBox();
            songGridView.Refresh();
            songGridView.Focus();

            settings.Save(SETTINGSPATH);
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
            settings.StartupPlaylist = library.Name;
            settings.Save(SETTINGSPATH);

            columnSettings = new ColumnSettings(songGridView.Columns);
            columnSettings.Save(COLUMNSETTINGSPATH);

            foreach (IPlaylist pl in playlists)
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
                if (showingPlaylist != library)
                {
                    showingPlaylist.Add(path);
                    UpdateShowingPlaylist(true);
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
            if (showingPlaylist == library)
                UpdateShowingPlaylist(true);
            PopulateShuffleQueue();
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
            albumArtBox.Image = Song.GetImage(currentlyPlaying);
        }

        private void UpdateSongInfoLabel()
        {
            if (songInfoLabel.InvokeRequired)
                songInfoLabel.Invoke(new MethodInvoker(delegate { UpdateSongInfoLabel(); }));
            else
            {
                if (musicPlayer.PlaybackState != PlaybackState.Stopped)
                    songInfoLabel.Text =
                          currentlyPlaying.Title + "\n"
                        + currentlyPlaying.Artist + "\n"
                        + currentlyPlaying.Album;
                else
                    songInfoLabel.Text = "";
            }
        }
        #endregion

        #region Playback control methods
        private void PlaySong(Song song, IPlaylist inPlaylist)
        {
            if (currentSongTimePlayed.Ticks > 0.95 * musicPlayer.Length.Ticks)
            {
                currentSongTimePlayed = TimeSpan.Zero;
                currentlyPlaying.LastPlayed = DateTime.Now;
                currentlyPlaying.PlayCount++;

                //SCROBBLE HERE IF ENABLED

                RefreshSongGridView();
            }

            if (ReloadSong(song))
            {
                if (inPlaylist == shuffleQueue)
                    PopulateShuffleQueue();

                ResetSearchBarTimer();
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
                playingPlaylist = inPlaylist;

                UpdateTrayIconText();
                UpdateSongImage();
                UpdateSongLengthLabel();
                UpdateSongInfoLabel();

                //Saves changed from tag editing when song is not playing any more
                if (songToSave != null)
                    if (songToSave.Path != currentlyPlaying.Path)
                    {
                        songToSave.SaveTags();
                        songToSave = null;
                    }
            }
        }

        private void RefreshSongGridView()
        {
            if (songGridView.InvokeRequired)
                songGridView.Invoke(new Action(delegate { songGridView.Refresh(); }));
            else
                songGridView.Refresh();
        }

        /// <summary>
        /// True if reaload was successful
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        private bool ReloadSong(Song song)
        {
            try
            {
                song.Reload();
            }
            catch (SongReloadException ex)
            {
                MessageBox.Show(ex.ToString() + "\nSong will be removed from the library.");
                library.Remove(song);
                return false;
            }
            return true;
        }

        private void PlayMusic()
        {
            if (volumeTrackBar.InvokeRequired)
                volumeTrackBar.Invoke(new Action(delegate { musicPlayer.Volume = volumeTrackBar.Value; }));
            else
                musicPlayer.Volume = volumeTrackBar.Value;

            musicPlayer.Play();

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
            albumArtBox.Image = null;
            searchBarTimer.Stop();
            musicPlayer.Stop();
            musicPlayer.Volume = 0;
            UpdateSongInfoLabel();
        }

        private void PlayNextSong()
        {
            Song nextSong = playingPlaylist.GetNext();
            if (nextSong != null)
                PlaySong(nextSong, playingPlaylist);
        }

        private void PlayPreviousSong()
        {
            Song previousSong = playingPlaylist.GetPrevious();
            if (previousSong != null)
                PlaySong(previousSong, playingPlaylist);
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
                if (songGridView.Rows.Count > showingPlaylist.CurrentIndex)
                    if (songGridView.Rows[showingPlaylist.CurrentIndex].DataBoundItem != null)
                        PlaySong(songGridView.Rows[showingPlaylist.CurrentIndex].DataBoundItem as Song, showingPlaylist);
            }
        }

        private void songGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                showingPlaylist.CurrentIndex = e.RowIndex;
                PlaySong((Song)songGridView.Rows[e.RowIndex].DataBoundItem, showingPlaylist);
            }
        }

        #region Menu items
        private void playPauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlayOrPause();
        }

        private void playNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlayNextSong();
        }

        private void playPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlayPreviousSong();
        }
        #endregion

        #endregion

        #region Local Hotkeys
        private void songGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (songGridView.Focused)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    PlaySong(songGridView.SelectedRows[0].DataBoundItem as Song, showingPlaylist);
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    DeleteSongs(songGridView.SelectedRows);
                }
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (musicPlayer.PlaybackState == PlaybackState.Playing)
                if (e.Control)
                    if (settings.RatingHotkeys.Contains(e.KeyCode))
                    {
                        RateSong(currentlyPlaying, Array.IndexOf(settings.RatingHotkeys, e.KeyCode));
                        
                        songGridView.Refresh();
                    }
        }

        private void RateSongs(DataGridViewSelectedRowCollection rows, int rating)
        {
            foreach (DataGridViewRow row in rows)
            {
                Song s = row.DataBoundItem as Song;
                s.Rating = rating;
                library.UpdateSongInfo(s);
            }
            songGridView.Refresh();
            showingPlaylist.Save();
        }

        private void RateSong(Song s, int rating)
        {
            s.Rating = rating;
            library.UpdateSongInfo(currentlyPlaying);
            songGridView.Refresh();
            playingPlaylist.Save();
        }

        private void DeleteSongs(DataGridViewSelectedRowCollection rows)
        {
            bool shouldDelete = true;
            if (showingPlaylist.GetType() == typeof(RatingFilterPlaylist))
                shouldDelete = false;
            if (rows.Count > 25)
            {
                string queryMessage = "You are about to delete " + rows.Count + " songs from the ";
                if (showingPlaylist != library)
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
                    showingPlaylist.RemoveAll();
                else
                {
                    List<int> indexList = GetSortedRowIndexes(rows);
                    foreach (int i in indexList)
                    {
                        if (currentlyPlaying != null)
                        {
                            if (showingPlaylist == library)
                            {
                                if (currentlyPlaying == (Song)songGridView.Rows[i].DataBoundItem)
                                    StopPlaying();
                            }
                            else
                            {
                                if (playingPlaylist.CurrentIndex == i)
                                    StopPlaying();
                            }
                        }
                    }
                    showingPlaylist.Remove(indexList);
                }

                if (showingPlaylist == shuffleQueue)
                    PopulateShuffleQueue();
                showingPlaylist.Save();
                UpdateShowingPlaylist(false);
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
            if (hook != null)
                hook.Dispose();

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
            }
        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            foreach (GlobalHotkey hk in settings.GlobalHotkeys)
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
            if (currentlyPlaying != null)
                SongInfoPopup.ShowPopup(currentlyPlaying, songInfoPopupTime);
        }
        #endregion

        #region Playlist manipulatiom methods
        private void playlistGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (playlistGridView.Focused)
                if (e.KeyCode == Keys.Delete)
                {
                    DeletePlaylist(playlistGridView.SelectedCells[0]);
                }
        }

        private void DeletePlaylist(DataGridViewCell cell)
        {
            bool deleted = false;
            IPlaylist pl = playlists[cell.RowIndex];
            if (pl != library && pl != shuffleQueue)
            {
                if (pl == showingPlaylist)
                    ChangeToPlaylist(library);
                if (pl == playingPlaylist)
                {
                    StopPlaying();
                    playingPlaylist = library;
                }
                playlists.Remove(pl);
                try
                {
                    File.Delete(pl.Path);
                }
                catch { }
                deleted = true;
            }
            
            if (deleted)
                SetPlaylistGridView();
        }

        private void RenamePlaylist()
        {
            this.renamePlaylist = GetPlaylist(playlistGridView.CurrentCell.Value.ToString()) as Playlist;
            playlistGridView.BeginEdit(true);
        }

        private void playlistGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string currentName = playlistGridView.CurrentCell.Value.ToString();
            string oldName = this.renamePlaylist.Name;
            string oldPath = this.renamePlaylist.Path;
            bool acceptable = true;
            foreach (IPlaylist pl in playlists)
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
                playlistGridView.BeginEdit(true);
        }
        #endregion

        #region Playlist selection
        private void playlistGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ChangeToPlaylist(playlists[e.RowIndex]);
        }

        private void ChangeToPlaylist(IPlaylist playlist)
        {
            if (showingPlaylist != playlist)
            {
                if (playlist != library)
                    ResetSearchBox();
                showingPlaylist.Save();
                showingPlaylist = playlist;
                songGridView.ClearSelection();
                playlistGridView.Rows[playlists.IndexOf(playlist)].Cells[0].Selected = true;
                SetSortGlyph();
                if (showingPlaylist == shuffleQueue)
                    PopulateShuffleQueue();
                UpdateShowingPlaylist(true);
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
            this.currentSongTimePlayed = TimeSpan.Zero;
            this.oldPosition = TimeSpan.Zero;
        }

        private void searchBarTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan length = musicPlayer.Length;
            TimeSpan position = musicPlayer.Position;

            UpdateSearchBar(length, position);
            UpdateCurrentTimeLabel();

            if (Math.Abs(position.Seconds - oldPosition.Seconds) < 5)
                currentSongTimePlayed += position - oldPosition;

            this.oldPosition = position;

            if ((searchBarTimer.Enabled && musicPlayer.PlaybackState == PlaybackState.Stopped) ||
                (searchBarTimer.Enabled && position > length))
            {
                StopPlaying();
                PlayNextSong();
            }
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

        private void UpdateCurrentTimeLabel()
        {
            if (currentTime_Label.InvokeRequired)
                currentTime_Label.Invoke(new MethodInvoker(delegate { UpdateCurrentTimeLabel(); }));
            else
            {
                if (!currentTime_Label.IsDisposed)
                    currentTime_Label.Text = Song.DurationFromTimeSpanToString(musicPlayer.Position);
            }
        }

        private void UpdateSongLengthLabel()
        {
            if (songInfoLabel.InvokeRequired)
                songInfoLabel.Invoke(new Action(delegate
                    {
                        songLength_Label.Text = Song.DurationFromTimeSpanToString(musicPlayer.Length);
                    }));
            else
                songLength_Label.Text = Song.DurationFromTimeSpanToString(musicPlayer.Length);
        }

        #region Set control value methods
        private void SetSearchBarValue(int value)
        {
            if (searchBar.InvokeRequired)
                searchBar.Invoke(new MethodInvoker(delegate { SetSearchBarValue(value); }));
            else
                searchBar.Value = value;
        }

        private void SetVolumeBarValue(int value)
        {
            if (this.volumeTrackBar.InvokeRequired)
                volumeTrackBar.Invoke(new MethodInvoker(delegate { SetVolumeBarValue(value); }));
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

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("KoPlayer 0.9\n(C) Karl-Oskar Smed, 2015\nhttps://github.com/koaset/KoPlayer");
        }
        #endregion

        #region Column header events
        private void songGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (showingPlaylist != shuffleQueue)
                {
                    showingPlaylist.Sort(e.ColumnIndex, songGridView.Columns[e.ColumnIndex].HeaderText);
                    UpdateShowingPlaylist(false);
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
            if (showingPlaylist.SortColumnIndex >= 0)
                songGridView.Columns[showingPlaylist.SortColumnIndex].HeaderCell.SortGlyphDirection = showingPlaylist.SortOrder;
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
            CreateNewPlaylist();
        }

        private void CreateNewPlaylist()
        {
            string name = GetNewPlaylistName(false);

            IPlaylist newPlaylist = new Playlist(library, name);
            playlists.Add(newPlaylist);
            SetPlaylistGridView();
            ChangeToPlaylist(newPlaylist);
        }

        string GetNewPlaylistName(bool filter)
        {
            string name = "";
            int i = 0;
            bool taken;
            do
            {
                taken = false;
                i++;

                if (!filter)
                    name = "New playlist " + i;
                else
                    name = "New filter playlist " + i;

                foreach (IPlaylist pl in playlists)
                {
                    if (pl.Name.ToLower() == name.ToLower())
                        taken = true;
                }
            }
            while (taken);

            return name;
        }

        private void newRatingFilterPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RatingFilterPlaylistPopup popup = new RatingFilterPlaylistPopup();
            popup.SetStartPosition();

            DialogResult dr = popup.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                RatingFilterInfo filterInfo = popup.GetResult();
                CreateNewRatingFilterPlaylist(filterInfo.AllowedRating, filterInfo.IncludeHigher);
            }
        }

        private void CreateNewRatingFilterPlaylist(int ratingCutoff, bool includeHigher)
        {
            string name = GetNewPlaylistName(true);

            IPlaylist newPlaylist = new RatingFilterPlaylist(library, name, ratingCutoff, includeHigher);
            playlists.Add(newPlaylist);
            SetPlaylistGridView();
            ChangeToPlaylist(newPlaylist);
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
            if (showingPlaylist != library)
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

                    IPlaylist pl = showingPlaylist;
                    if (pl != library)
                    {
                        List<int> indexes = GetSortedRowIndexes(rows);
                        foreach (int index in indexes)
                            pl.Remove(index);
                        foreach (DataGridViewRow row in rows)
                        {
                            Song s = (Song)row.DataBoundItem;
                            Playlist playlist = pl as Playlist;
                            playlist.Insert(info.RowIndex, s);
                        }

                        if (showingPlaylist == shuffleQueue)
                            PopulateShuffleQueue();
                        else
                            songGridView.DataSource = pl.GetSongs();
                    }
                }
            }
        }

        private void playlistGridView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void playlistGridView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DataGridViewSelectedRowCollection)))
            {
                Point p = playlistGridView.PointToClient(new Point(e.X, e.Y));
                DataGridView.HitTestInfo info = playlistGridView.HitTest(p.X, p.Y);
                if (info.RowIndex >= 0)
                {
                    DataGridViewSelectedRowCollection rows;
                    rows = (DataGridViewSelectedRowCollection)e.Data.GetData(typeof(DataGridViewSelectedRowCollection));

                    IPlaylist pl = GetPlaylist(playlistGridView.Rows[info.RowIndex].Cells[0].Value.ToString());
                    if (pl != library)
                    {
                        foreach (DataGridViewRow row in rows)
                        {
                            Song s = (Song)row.DataBoundItem;
                            pl.Add(s.Path);
                        }
                        if (pl == showingPlaylist)
                            UpdateShowingPlaylist(true);
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
            this.playlistGridView.Update();
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
                UpdateShowingPlaylist(true);
                ResetSearchBox();
            }
        }

        private void ResetSearchBox()
        {
            if (searchBox.InvokeRequired)
                searchBox.Invoke(new MethodInvoker(delegate { ResetSearchBox(); }));
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
            ChangeToPlaylist(library);
            if (searchBox.Text.Length > 0)
            {
                songGridView.DataSource = library.Search(searchBox.Text);
                UpdateShowingPlaylist(false);
            }
            else if (searchBox.Text.Length == 0)
                UpdateShowingPlaylist(true);
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
                    UpdateShowingPlaylist(true);
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
        private void playlistGridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ContextMenu cm = new ContextMenu();
                cm.MenuItems.Add(CreateMenuItem("New playlist", newPlaylistToolStripMenuItem_Click));
                cm.MenuItems.Add(CreateMenuItem("New rating filter playlist",
                    newRatingFilterPlaylistToolStripMenuItem_Click));

                clickedPlaylistIndex = playlistGridView.HitTest(e.X, e.Y).RowIndex;

                if (clickedPlaylistIndex >= 0)
                {
                    playlistGridView.ClearSelection();
                    playlistGridView.Rows[clickedPlaylistIndex].Selected = true;
                    playlistGridView.Rows[clickedPlaylistIndex].Cells[0].Selected = true;

                    if (clickedPlaylistIndex > 1)
                    {
                        if (playlists[clickedPlaylistIndex].GetType() == typeof(RatingFilterPlaylist))
                            cm.MenuItems.Add("Edit rating filter playlist", editRatingFilterPlaylist);
                        cm.MenuItems.Add(CreateMenuItem("Rename playlist", playlistGridViewRightClickRename));
                        cm.MenuItems.Add(CreateMenuItem("Delete playlist", playlistGridViewRightClickMenuDelete));
                    }
                }
                cm.Show(playlistGridView, new Point(e.X, e.Y));
            }
        }

        private void playlistGridViewRightClickMenuDelete(object sender, EventArgs e)
        {
            DeletePlaylist(playlistGridView.SelectedCells[0]);
        }

        private void playlistGridViewRightClickRename(object sender, EventArgs e)
        {
            RenamePlaylist();
        }

        private void editRatingFilterPlaylist(object sender, EventArgs e)
        {
            RatingFilterPlaylist clickedPlaylist = playlists[clickedPlaylistIndex] as RatingFilterPlaylist;
            if (clickedPlaylist != null)
            {
                RatingFilterPlaylistPopup popup = new RatingFilterPlaylistPopup(clickedPlaylist);
                popup.SetStartPosition();

                DialogResult dr = popup.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    RatingFilterInfo filterInfo = popup.GetResult();
                    clickedPlaylist.AllowedRating = filterInfo.AllowedRating;
                    clickedPlaylist.IncludeHigher = filterInfo.IncludeHigher;
                    if (showingPlaylist != clickedPlaylist)
                        ChangeToPlaylist(clickedPlaylist);
                    else
                        UpdateShowingPlaylist(true);
                }
            }
        }
        #endregion

        #region Song gridview right click menu

        private void songGridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                this.clickedSongIndex = songGridView.HitTest(e.X, e.Y).RowIndex;
                if (this.clickedSongIndex >= 0)
                {
                    if (songGridView.Rows[this.clickedSongIndex].Cells[0].Selected != true)
                        songGridView.ClearSelection();
                    songGridView.Rows[this.clickedSongIndex].Cells[0].Selected = true;

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
            cm.MenuItems.Add(CreateMenuItem("Queue next in shuffle queue", songGridViewRightClickAddToShuffleQueueNext));
            cm.MenuItems.Add(CreateMenuItem("Add songs to shuffle queue", songGridViewRightClickAddToShuffleQueueBottom));
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
            cm.MenuItems.Add(CreateMenuItem("Show file in explorer", songGridViewRightClickShowExplorer));
            if (showingPlaylist.GetType() != typeof(RatingFilterPlaylist))
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
            PlaySong((Song)songGridView.Rows[clickedSongIndex].DataBoundItem, showingPlaylist);
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

        private void songGridViewRightClickAddToShuffleQueueNext(object sender, EventArgs e)
        {
            List<Song> songs = new List<Song>();
            foreach (DataGridViewRow row in songGridView.SelectedRows)
                songs.Add((Song)row.DataBoundItem);
            shuffleQueue.Insert(shuffleQueue.CurrentIndex + 1, songs);

            if (showingPlaylist == shuffleQueue)
                UpdateShowingPlaylist(true);
        }

        private void songGridViewRightClickAddToShuffleQueueBottom(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in songGridView.SelectedRows)
                shuffleQueue.Add((Song)row.DataBoundItem);

            if (showingPlaylist == shuffleQueue)
                UpdateShowingPlaylist(true);
        }

        private void songGridViewRightClickShowExplorer(object sender, EventArgs e)
        {
            Song clickedSong = songGridView.Rows[clickedSongIndex].DataBoundItem as Song;
            Process.Start("explorer.exe", @"/select, " + "\"" + clickedSong.Path + "\"");
        }

        private void songGridViewRightClickDelete(object sender, EventArgs e)
        {
            DeleteSongs(songGridView.SelectedRows);
        }

        private void songGridViewRightClickProperties(object sender, EventArgs e)
        {
            Song clickedSong = songGridView.Rows[clickedSongIndex].DataBoundItem as Song;
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
                SongPropertiesWindow popUp = new SongPropertiesWindow(this, clickedSong,
                    this.clickedSongIndex, this.showingPlaylist, this.library);
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
