using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.Streams;
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
        private const string EQUALIZERPATH = @"Default.eq";
        #endregion

        #region Properties
        public Settings Settings { get { return settings; } set { settings = value; } }
        public Song CurrentlyPlaying { get { return this.currentlyPlaying; } }
        #endregion

        #region Fields
        private Library library;
        private Playlist shuffleQueue;
        
        private IPlaylist showingPlaylist;
        private IPlaylist playingPlaylist;
        private List<IPlaylist> playlists;

        private Settings settings;
        private ColumnSettings columnSettings;
        private KeyboardHook hook;
        private int songInfoPopupTime = 3000;

        private readonly MusicPlayer musicPlayer = new MusicPlayer();
        private MMDevice defaultAudioDevice;
        private EqualizerSettings equalizerSettings;

        private Song currentlyPlaying;
        private System.Timers.Timer searchBarTimer;
        private int searchBarTimerInterval = 100;
        private bool stopSearchBarUpdate = false;
        private TimeSpan oldPosition;
        private TimeSpan currentSongTimePlayed;
        private Song songToSave;

        private string searchBoxDefault = "Search Library";
        private System.Timers.Timer searchLibraryTimer;
        private int searchLibraryTimerInterval = 500;

        private Playlist tempPlaylist;
        private int clickedSongIndex;
        private int clickedPlaylistIndex;

        ToolStripProgressBar progressBar;
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
            library.LibraryChanged += library_LibraryChanged;

            settings = Settings.Load(SETTINGSPATH);
            if (settings == null)
                settings = new Settings();

            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            defaultAudioDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            this.musicPlayer.OpenCompleted += equalizerSettings_ShouldSet;

            this.equalizerSettings = EqualizerSettings.Load(EQUALIZERPATH);
            if (this.equalizerSettings == null)
            {
                this.equalizerSettings = new EqualizerSettings();
                this.equalizerSettings.Save(EQUALIZERPATH);
            }
            this.equalizerSettings.ValueChanged += equalizerSettings_ShouldSet;

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
            songGridView.Columns["Length"].DefaultCellStyle.Format = Song.LengthFormat;

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
            SetTrayIconContextMenu();
            SetStatusStrip();
        }

        private void SetSongGridViewStyle()
        {
            songGridView.RowTemplate.Height = settings.RowHeight;
            songGridView.DefaultCellStyle.Font = new Font(settings.FontName, settings.FontSize, GraphicsUnit.Point);
            songGridView.ShowCellToolTips = false;
        }

        private void SetStatusStrip()
        {
            ToolStripLabel item = new ToolStripLabel();
            item.Name = "playlist_info";
            item.Text = showingPlaylist.ToString();
            statusStrip1.Items.Add(item);
            ToolStripSeparator separator = new ToolStripSeparator();
            separator.Name = "playlist_info_separator";
            separator.Visible = false;
            statusStrip1.Items.Add(separator);

            item = new ToolStripLabel();
            item.Name = "playback_status";
            statusStrip1.Items.Add(item);
        }

        #endregion

        #region Playlist methods
        private void UpdateShowingPlaylist(bool getAll)
        {
            if (showingPlaylist != null)
            {
                if (songGridView.InvokeRequired)
                    songGridView.Invoke(new MethodInvoker(delegate { UpdateShowingPlaylist(getAll); }));
                else
                {
                    if (getAll)
                        songGridView.DataSource = showingPlaylist.GetAllSongs();
                    else
                        songGridView.DataSource = showingPlaylist.GetSongs();
                    if (showingPlaylist == shuffleQueue)
                        SetShuffleQueueColors();
                }
            }
        }

        private void UpdateFilterPlaylistSongPaths()
        {
            foreach (IPlaylist pl in playlists)
                if (pl.GetType() == typeof(RatingFilterPlaylist))
                    pl.GetAllSongs();
        }

        private void LoadPlaylists()
        {
            playlists = new List<IPlaylist>();
            playlists.Add(library);

            if (!Directory.Exists(PLAYLISTDIRECTORYPATH))
                Directory.CreateDirectory(PLAYLISTDIRECTORYPATH);

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

            playlistGridView.ClearSelection();
            playlistGridView.Rows[playlists.IndexOf(showingPlaylist)].Cells[0].Selected = true;
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

        #region Playlist manipulatiom methods
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

        private void playlistGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (playlistGridView.Focused)
                if (e.KeyCode == Keys.Delete)
                {
                    DeletePlaylist(playlistGridView.SelectedCells[0]);
                }
        }

        private void RenamePlaylist()
        {
            this.tempPlaylist = GetPlaylist(playlistGridView.CurrentCell.Value.ToString()) as Playlist;
            playlistGridView.BeginEdit(true);
        }

        private void playlistGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string currentName = playlistGridView.CurrentCell.Value.ToString();
            string oldName = this.tempPlaylist.Name;
            string oldPath = this.tempPlaylist.Path;
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
                    tempPlaylist.Name = currentName;
                    try
                    {
                        System.IO.File.Move(oldPath, tempPlaylist.Path);
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
                showingPlaylist = playlist;
                SetSortGlyph();
                if (showingPlaylist == shuffleQueue)
                    PopulateShuffleQueue();
                UpdateShowingPlaylist(true);
                songGridView.ClearSelection();
                SetPlaylistGridView();
                UpdateStatusStip();
            }
        }
        #endregion

        #endregion

        #region Settings window
        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hook.Dispose();
            SettingsWindow settingsWindow = new SettingsWindow(this, this.playlists);
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
            RefreshSongGridView();
            songGridView.Focus();

            settings.Save(SETTINGSPATH);
        }
        #endregion

        #region Exit
        private void OnExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.FormWidth = this.Width;
            settings.FormHeight = this.Height;
            settings.StartupPlaylist = showingPlaylist.Name;
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
            trayIcon.Dispose();
            musicPlayer.Dispose();
        }
        #endregion

        #region Add songs
        private void addsongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Audio Files (*.mp3,*m4a,*.wma,*.aac,*.flac)|*.mp3;*m4a;*.wma;*.aac;*.flac;|" +
                        "MP3 files (*.mp3)|*.mp3|" +
                        "M4A files (*.m4a)|*.m4a|" +
                        "WMA files (*.wma)|*.wma|" +
                        "AAC files (*.aac)|*.aac|" +
                        "FLAC files (*.flac)|*.flac";

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
                library.Add(GetSongsPathsFromFolder(dialog.SelectedPath));
                SetAddFilesStatusStrip();
            }
        }

        private List<string> GetSongsPathsFromFolder(string folderPath)
        {
            List<string> musicFiles = new List<string>();
            foreach (string extension in Library.EXTENSIONS)
                musicFiles.AddRange(Directory.GetFiles(folderPath, "*" + extension, SearchOption.AllDirectories));
            return musicFiles;
        }

        private void SetAddFilesStatusStrip()
        {
            statusStrip1.Items.Add(new ToolStripSeparator());
            statusStrip1.Items.Add("Adding files...");
            progressBar = new ToolStripProgressBar("add_progress");
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            statusStrip1.Items.Add(progressBar);
            UpdateStatusStip();
            library.ReportProgress += library_ReportProgress;
        }

        private void library_LibraryChanged(object sender, LibraryChangedEventArgs e)
        {
            foreach (IPlaylist pl in playlists)
                if (pl.GetType() == typeof(RatingFilterPlaylist))
                    pl.GetAllSongs();

            if (showingPlaylist != shuffleQueue)
                UpdateShowingPlaylist(true);
            else
                PopulateShuffleQueue();

            UpdateStatusStip();
        }

        private void library_ReportProgress(object sender, ReportProgressEventArgs e)
        {
            if (e.ProgressPercentage < 100)
            {
                this.progressBar.Value = (int)(e.ProgressPercentage);
            }
            else if (e.ProgressPercentage == 100)
            {
                statusStrip1.Items.Clear();
                progressBar = null;
                SetStatusStrip();
                library.ReportProgress -= library_ReportProgress;
            }
        }
        #endregion

        #region Control updates

        private void RefreshSongGridView()
        {
            if (songGridView.InvokeRequired)
                songGridView.Invoke(new MethodInvoker(delegate { RefreshSongGridView(); }));
            else
                songGridView.Refresh();
        }

        private void UpdateStatusStip()
        {
            if (statusStrip1.InvokeRequired)
                statusStrip1.Invoke(new MethodInvoker(delegate { UpdateStatusStip(); }));
            else
            {
                statusStrip1.Items["playlist_info"].Text = showingPlaylist.ToString();

                string itemText = "";
                if (musicPlayer.PlaybackState == PlaybackState.Playing)
                    itemText = "Playing";
                else if (musicPlayer.PlaybackState == PlaybackState.Paused)
                    itemText = "Paused";
                statusStrip1.Items["playback_status"].Text = itemText;

                statusStrip1.Items["playlist_info_separator"].Visible = !(itemText.Length == 0);
            }
        }

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

        private void SetTrayIconContextMenu()
        {
            ContextMenu cm = new ContextMenu();
            if (currentlyPlaying != null)
            {
                string itemText = "Play";
                if (musicPlayer.PlaybackState == PlaybackState.Playing)
                    itemText = "Pause";
                cm.MenuItems.Add(CreateMenuItem(itemText, playpauseButton_Click));

                cm.MenuItems.Add(CreateMenuItem("Play next", playNextButton_Click));
                cm.MenuItems.Add(CreateMenuItem("Play previous", playPreviousButton_Click));
            }
            cm.MenuItems.Add(CreateMenuItem("Show KoPlayer", showKoPlayer_Event));
            cm.MenuItems.Add(CreateMenuItem("Close", OnExitButton_Click));
            trayIcon.ContextMenu = cm;
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

        #region Playback control
        private void PlaySong(Song song, IPlaylist inPlaylist)
        {
            if (currentSongTimePlayed.Ticks > 0.95 * musicPlayer.Length.Ticks)
            {
                this.currentSongTimePlayed = TimeSpan.Zero;
                this.currentlyPlaying.LastPlayed = DateTime.Now;
                this.currentlyPlaying.PlayCount++;

                //SCROBBLE HERE IF ENABLED

                RefreshSongGridView();
            }

            if (ReloadSong(song))
            {
                ResetSearchBarTimer();
                searchBarTimer.Stop();

                try
                {
                    this.musicPlayer.Open(song.Path, defaultAudioDevice);
                    PlayMusic();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Play music exception: " + ex.ToString());
                }

                this.currentlyPlaying = song;
                this.playingPlaylist = inPlaylist;

                UpdateTrayIconText();
                UpdateSongImage();
                UpdateSongLengthLabel();
                UpdateSongInfoLabel();

                if (inPlaylist == shuffleQueue)
                    PopulateShuffleQueue();

                //Saves changed from tag editing when song is not playing any more
                if (songToSave != null)
                    if (songToSave.Path != currentlyPlaying.Path)
                    {
                        songToSave.SaveTags();
                        songToSave = null;
                    }
            }
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
                OnSongReloadException(ex, song);
                return false;
            }
            return true;
        }

        private void OnSongReloadException(SongReloadException ex, Song song)
        {
            MessageBox.Show(ex.ToString() + "\nSong will be removed from the library.");
            library.Remove(song);
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
            UpdateStatusStip();
            UpdatePlayPauseButtonImage();
        }

        private void PauseMusic()
        {
            musicPlayer.Volume = 0;
            musicPlayer.Pause();
            searchBarTimer.Stop();
            UpdateStatusStip();
            UpdatePlayPauseButtonImage();
        }

        private void StopPlaying()
        {
            albumArtBox.Image = null;
            searchBarTimer.Stop();
            musicPlayer.Stop();
            musicPlayer.Volume = 0;
            UpdateSongInfoLabel();
            UpdateStatusStip();
            UpdatePlayPauseButtonImage();
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

        private void playNextButton_Click(object sender, EventArgs e)
        {
            PlayNextSong();
        }

        private void playPreviousButton_Click(object sender, EventArgs e)
        {
            PlayPreviousSong();
        }
        #endregion

        #endregion

        #region Playback timer
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
            TimeSpan length = currentlyPlaying.Length;
            TimeSpan position = musicPlayer.Position;

            UpdateSearchBar(length, position);
            UpdateCurrentTimeLabel();

            if (Math.Abs(position.Seconds - oldPosition.Seconds) < 5)
                currentSongTimePlayed += position - oldPosition;

            this.oldPosition = position;

            if (searchBarTimer.Enabled)
                if (musicPlayer.PlaybackState == PlaybackState.Stopped || position > length)
                {
                    StopPlaying();
                    PlayNextSong();
                }
        }
        #endregion

        #region Hotkeys
        #region Local Hotkeys
        private void songGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (songGridView.Focused)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (songGridView.SelectedRows.Count == 1)
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
                        RefreshSongGridView();
                    }
        }

        private void RateSongs(DataGridViewSelectedRowCollection rows, int rating)
        {
            foreach (DataGridViewRow row in rows)
            {
                Song s = row.DataBoundItem as Song;
                s.Rating = rating;
            }

            library.ResetSearchDictionaries();

            UpdateFilterPlaylistSongPaths();
            RefreshSongGridView();
            showingPlaylist.Save();
            if (showingPlaylist.GetType() == typeof(RatingFilterPlaylist))
                UpdateShowingPlaylist(true);
        }

        private void RateSong(Song s, int rating)
        {
            s.Rating = rating;
            library.UpdateSongInfo(currentlyPlaying);
            UpdateSongForRatingFilterPlaylist(s);
            UpdateFilterPlaylistSongPaths();
            RefreshSongGridView();
            playingPlaylist.Save();
            if (showingPlaylist.GetType() == typeof(RatingFilterPlaylist))
                UpdateShowingPlaylist(true);
        }

        private void UpdateSongForRatingFilterPlaylist(Song song)
        {
            foreach (IPlaylist pl in playlists)
                pl.UpdateSongInfo(song);
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
                if (songGridView.SelectedRows.Count == showingPlaylist.NumSongs)
                {
                    showingPlaylist.RemoveAll();
                    if (musicPlayer.PlaybackState != PlaybackState.Stopped)
                        StopPlaying();
                }
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
        #endregion

        #region Playback dependant method updates
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
                    currentTime_Label.Text = musicPlayer.Position.ToString(Song.LengthFormat);
            }
        }

        private void UpdateSongLengthLabel()
        {
            if (songInfoLabel.InvokeRequired)
                songInfoLabel.Invoke(new Action(delegate
                    {
                        songLength_Label.Text = currentlyPlaying.LengthString;
                    }));
            else
                songLength_Label.Text = currentlyPlaying.LengthString;
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
            MessageBox.Show("KoPlayer 0.9\n(C) Karl-Oskar Smed, 2015\nhttps://github.com/koaset/KoPlayer\n Icons from: https://icons8.com/");
        }
        #endregion

        #region Sorting
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
            {
                UpdateShowingPlaylist(true);
                library.ResetSearchDictionaries();
            }
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
            ShowCreateNewRatingFilterPlaylistPopup();
        }

        private void ShowCreateNewRatingFilterPlaylistPopup()
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
                DragDropSongs data = new DragDropSongs(DataFormats.FileDrop, e.Item);
                songGridView.DoDragDrop(data, DragDropEffects.Copy);
            }
        }

        private void songGridView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.Data.GetType() == typeof(DragDropSongs))
                {
                    if (showingPlaylist.GetType() == typeof(Playlist))
                        e.Effect = DragDropEffects.Copy;
                }
                else if (CanAddPaths((string[])e.Data.GetData(DataFormats.FileDrop)))
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            }
        }

        private void songGridView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.Data.GetType() == typeof(DragDropSongs))
                    HandleSongDragDrop(e);
                else
                    HandleFileDragDrop(e);
            }
        }

        private void HandleSongDragDrop(DragEventArgs e)
        {
            Point p = songGridView.PointToClient(new Point(e.X, e.Y));
            DataGridView.HitTestInfo info = songGridView.HitTest(p.X, p.Y);
            if (info.RowIndex >= 0)
            {
                DataGridViewSelectedRowCollection rows = songGridView.SelectedRows;
                IPlaylist pl = showingPlaylist;

                if (pl != library)
                {
                    List<int> indexes = GetSortedRowIndexes(rows);
                    List<Song> songs = new List<Song>();

                    foreach (int index in indexes)
                    {
                        songs.Add((Song)songGridView.Rows[index].DataBoundItem);
                        pl.Remove(index);
                    }

                    foreach (Song s in songs)
                    {
                        Playlist playlist = pl as Playlist;
                        playlist.Insert(info.RowIndex, s);
                    }

                    if (showingPlaylist == shuffleQueue)
                        PopulateShuffleQueue();
                    else
                        UpdateShowingPlaylist(true);
                }
            }
        }

        private void HandleFileDragDrop(DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (CanAddPaths(paths))
            {
                List<string> songPaths = new List<string>();
                foreach (string path in paths)
                {
                    if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                        songPaths.AddRange(GetSongsPathsFromFolder(path));
                    else
                        songPaths.Add(path);
                }

                library.Add(songPaths);
                SetAddFilesStatusStrip();
                ChangeToPlaylist(library);
            }
        }

        private bool CanAddPaths(string[] paths)
        {
            if (paths == null || paths.Length <= 0)
                return false;
            foreach (string file in paths)
            {
                if (!Library.EXTENSIONS.Contains(Path.GetExtension(file.ToLower())))
                    if (!File.GetAttributes(file).HasFlag(FileAttributes.Directory))
                        return false;
            }
            return true;
        }

        private void playlistGridView_DragOver(object sender, DragEventArgs e)
        {
            Point p = playlistGridView.PointToClient(new Point(e.X, e.Y));
            DataGridView.HitTestInfo info = playlistGridView.HitTest(p.X, p.Y);

            e.Effect = DragDropEffects.None;

            if (info.RowIndex >= 0)
            {
                IPlaylist pl = playlists[info.RowIndex];

                if (pl.GetType() == typeof(Playlist))
                    e.Effect = DragDropEffects.Copy;
            }
        }

        private void playlistGridView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetType() == typeof(DragDropSongs))
            {
                DragDropSongs data = (DragDropSongs)e.Data;
                data.ReturnPaths = false;

                Point p = playlistGridView.PointToClient(new Point(e.X, e.Y));
                DataGridView.HitTestInfo info = playlistGridView.HitTest(p.X, p.Y);

                if (info.RowIndex >= 0)
                {
                    DataGridViewSelectedRowCollection rows = songGridView.SelectedRows;

                    List<Song> songs = new List<Song>();
                        foreach (DataGridViewRow row in rows)
                             songs.Add((Song)row.DataBoundItem);

                    IPlaylist pl = GetPlaylist(playlistGridView.Rows[info.RowIndex].Cells[0].Value.ToString());
                    if (pl.GetType() == typeof(Playlist))
                    {
                        pl.Add(songs);

                        if (pl == showingPlaylist)
                            UpdateShowingPlaylist(true);
                    }
                }
            }
        }
        #endregion

        #region Minimize to tray

        private void showKoPlayer_Event(object sender, EventArgs e)
        {
            ShowKoPlayer();
        }

        private void ShowKoPlayer()
        {
            if (this.WindowState == FormWindowState.Minimized)
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
            else
                this.Activate();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                trayIcon.Visible = true;
                this.Hide();
            }
        }

        private void trayIcon_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                SetTrayIconContextMenu();
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
                cm.MenuItems.Add(CreateMenuItem("Pause", playpauseButton_Click));
            if (musicPlayer.PlaybackState == PlaybackState.Paused)
                cm.MenuItems.Add(CreateMenuItem("Resume", playpauseButton_Click));
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
            List<Song> songs = new List<Song>();
            
            if (songGridView.SelectedRows.Count > 1)
            {
                foreach (DataGridViewRow row in songGridView.SelectedRows)
                    songs.Add((Song)row.DataBoundItem);
            }
            else if (songGridView.SelectedRows.Count == 1)
            {
                Song clickedSong = songGridView.Rows[clickedSongIndex].DataBoundItem as Song;
                songs.Add(clickedSong);
            }

            ShowSongListProperties(songs);
            RefreshSongGridView();
        }

        private void ShowSongListProperties(List<Song> songs)
        {
            bool exists = true;
            foreach (Song s in songs)
            {
                try
                {
                    s.Reload();
                }
                catch (SongReloadException ex)
                {
                    OnSongReloadException(ex, s);
                    exists = false;
                }
            }
            if (exists && songs.Count > 0)
            {
                Form popup;

                if (songs.Count > 1)
                {
                    MultiSongPropertiesWindow multiSongWindow = new MultiSongPropertiesWindow(this, songs,
                        this.showingPlaylist, this.library);
                    multiSongWindow.SavePlayingSong += popup_SavePlayingSong;
                    popup = multiSongWindow;
                }
                else
                {
                    SongPropertiesWindow singleSongWindow = new SongPropertiesWindow(this, songs[0],
                        this.clickedSongIndex, this.showingPlaylist, this.library);
                    singleSongWindow.SavePlayingSong += popup_SavePlayingSong;
                    popup = singleSongWindow;
                }

                popup.StartPosition = FormStartPosition.CenterParent;
                popup.ShowDialog();
            }
        }

        private void popup_SavePlayingSong(object sender, SavePlayingSongEventArgs e)
        {
            songToSave = e.savingSong;
        }
        #endregion

        #endregion

        #endregion

        #region Button Image selection

        #region PlayPauseButton

        private void UpdatePlayPauseButtonImage()
        {
            if (musicPlayer.PlaybackState == PlaybackState.Playing)
            {
                //playpauseButton.Location = new Point(playpauseButton.Location.X - 5, playpauseButton.Location.Y);
                playpauseButton.ImageList = pauseButton_imageList;
            }
            else
            {
                //playpauseButton.Location = new Point(playpauseButton.Location.X + 5, playpauseButton.Location.Y);
                playpauseButton.ImageList = playButton_imageList;
            }
        }

        private void playpauseButton_MouseEnter(object sender, EventArgs e)
        {
            playpauseButton.ImageIndex = 1;
        }

        private void playpauseButton_MouseLeave(object sender, EventArgs e)
        {
            playpauseButton.ImageIndex = 0;
        }

        private void playpauseButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                playpauseButton.ImageIndex = 2;
        }

        private void playpauseButton_MouseUp(object sender, MouseEventArgs e)
        {
            playpauseButton.ImageIndex = 1;
        }
        #endregion

        #region Next Button
        private void nextButton_MouseEnter(object sender, EventArgs e)
        {
            nextButton.ImageIndex = 1;
        }

        private void nextButton_MouseLeave(object sender, EventArgs e)
        {
            nextButton.ImageIndex = 0;
        }

        private void nextButton_MouseDown(object sender, MouseEventArgs e)
        {
            nextButton.ImageIndex = 2;
        }

        private void nextButton_MouseUp(object sender, MouseEventArgs e)
        {
            nextButton.ImageIndex = 1;
        }
        #endregion

        #region Previous Button
        private void previousButton_MouseEnter(object sender, EventArgs e)
        {
            previousButton.ImageIndex = 1;
        }

        private void previousButton_MouseLeave(object sender, EventArgs e)
        {
            previousButton.ImageIndex = 0;
        }

        private void previousButton_MouseDown(object sender, MouseEventArgs e)
        {
            previousButton.ImageIndex = 2;
        }

        private void previousButton_MouseUp(object sender, MouseEventArgs e)
        {
            previousButton.ImageIndex = 1;
        }
        #endregion

        #endregion

        #region Equalizer

        private void equalizerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EqualizerWindow eqWindow = new EqualizerWindow(equalizerSettings);
            eqWindow.Show();
        }

        void equalizerSettings_ShouldSet(object sender, EventArgs e)
        {
            equalizerSettings.SetEqualizer(musicPlayer.Equalizer);
        }

        #endregion
    }
}
