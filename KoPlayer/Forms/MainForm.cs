﻿using CSCore.SoundOut;
using KoPlayer.Lib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace KoPlayer.Forms
{
    public partial class MainForm : Form
    {
        #region Path variables
        public static string ApplicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string SettingsPath = Path.Combine(ApplicationPath, @"Settings\settings.xml");
        public static string PlaylistDirectoryPath = Path.Combine(ApplicationPath, @"Playlists\");
        public static string ShuffleQueuePath = Path.Combine(ApplicationPath, @"Playlists\Shuffle Queue.pl");
        private static string ColumnSettingsPath = Path.Combine(ApplicationPath, @"Settings\column_settings.xml");
        private static string DefaultColumnSettingsPath = Path.Combine(ApplicationPath, @"Settings\default_column_settings.xml");
        private static string EqualizerPath = Path.Combine(ApplicationPath, @"Default.eq");
        public static string NowPlayingPath = Path.Combine(ApplicationPath, @"now_playing.txt");
        #endregion

        #region Properties
        public Settings Settings { get { return settings; } set { settings = value; } }
        public Song CurrentlyPlaying { get { return currentlyPlaying; } }
        public LastfmHandler LastFMHandler { get { return lfmHandler; } }
        public List<PlaylistBase> Playlists { get { return playlists; } }
        public ShuffleQueue ShuffleQueue { get { return shuffleQueue; } }
        #endregion

        #region Fields
        private Library library;
        private ShuffleQueue shuffleQueue;
        private Timer saveTimer;
        
        private PlaylistBase showingPlaylist;
        private PlaylistBase playingPlaylist;
        private List<PlaylistBase> playlists;

        private Settings settings;
        private ColumnSettings columnSettings;
        private KeyboardHook hook;
        private int songInfoPopupTime = 3000;

        private readonly MusicPlayer musicPlayer = new MusicPlayer();
        private EqualizerSettings equalizerSettings;

        private Song currentlyPlaying;
        private System.Timers.Timer searchBarTimer;
        private int searchBarTimerInterval = 100;
        private bool stopSearchBarUpdate = false;
        private TimeSpan oldPosition;
        private TimeSpan currentSongTimePlayed;
        private Song songToSave;
        private Image currentAlbumArt;

        private string searchBoxDefault = "Search Library";
        private PlaylistBase returnPlaylist;
        private System.Timers.Timer searchLibraryTimer;
        private int searchLibraryTimerInterval = 500;
        bool shouldSearch = false;

        private Playlist renamingPlaylist;
        private int clickedSongIndex;
        private int clickedPlaylistIndex;

        ToolStripProgressBar progressBar;

        private string startupRegKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        private LastfmHandler lfmHandler;

        private string aboutMessage = "KoPlayer 0.9\n(C) Karl-Oskar Smed, 2015" + 
            "\nhttps://github.com/koaset/KoPlayer\n Icons from: https://icons8.com/";
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
            library.Changed += library_LibraryChanged;

            settings = Settings.Load(SettingsPath) ?? new Settings();

            musicPlayer.SetDevice(settings.DeviceName);
            musicPlayer.OpenCompleted += equalizerSettings_ShouldSet;
            musicPlayer.OpenCompleted += musicPlayer_ShouldPlay;
            musicPlayer.DeviceVolume = settings.DeviceVolume;

            equalizerSettings = EqualizerSettings.Load(EqualizerPath);
            if (equalizerSettings == null)
            {
                equalizerSettings = new EqualizerSettings();
                equalizerSettings.Save(EqualizerPath);
            }
            equalizerSettings.ValueChanged += equalizerSettings_ShouldSet;

            SetUpGlobalHotkeys();

            lfmHandler = new LastfmHandler();
            if (settings.ScrobblingEnabled)
                lfmHandler.ResumeSessionAsync();

            InitializeComponent();
            SetControlReferences();
            SetupSaveTimer();
        }

        private void SetControlReferences()
        {
            volumeTrackBar.Player = musicPlayer;
            volumeTrackBar.SongGrid = songGridView;
        }

        private void SetupSaveTimer()
        {
            saveTimer = new Timer();
            saveTimer.Interval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            saveTimer.Tick += saveTimer_Elapsed;
            saveTimer.Start();
        }

        private void saveTimer_Elapsed(object sender, EventArgs e)
        {
            library?.Save();
            ShuffleQueue?.Save();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            trayIcon.Icon = Icon;
            Width = settings.FormWidth;
            Height = settings.FormHeight;

            ResetSearchBarTimer();
            LoadPlaylists();

            showingPlaylist = GetPlaylist(settings.StartupPlaylist) ?? library;
            playingPlaylist = showingPlaylist;

            SetSongGridViewStyle();

            UpdateShowingPlaylist();
            songGridView.AutoGenerateColumns = false;
            songGridView.Columns["Length"].DefaultCellStyle.Format = Song.LengthFormat;

            columnSettings = ColumnSettings.Load(ColumnSettingsPath) 
                ?? ColumnSettings.Load(DefaultColumnSettingsPath) ?? new ColumnSettings(songGridView.Columns);

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

            playlistGridView.ClearSelection();
            // Select proper playlist in gridview
            foreach (DataGridViewRow row in playlistGridView.Rows)
                if ((row.DataBoundItem as PlaylistBase).Name == settings.StartupPlaylist)
                    row.Cells[0].Selected = true;

            shuffleQueue.Source = GetPlaylist(settings.Shufflequeue_SourcePlaylistName);
            shuffleQueue.Populate();
            UpdateTrayIconText();
            SetTrayIconContextMenu();
            SetStatusStrip();
            UpdateShowingPlaylist();
            volumeTrackBar.Value = settings.Volume;
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
            statusStrip.Items.Add(item);
            ToolStripSeparator separator = new ToolStripSeparator();
            separator.Name = "playlist_info_separator";
            separator.Visible = false;
            statusStrip.Items.Add(separator);

            item = new ToolStripLabel();
            item.Name = "playback_status";
            statusStrip.Items.Add(item);
        }

        #endregion

        #region Playlist methods
        private void UpdateShowingPlaylist()
        {
            if (showingPlaylist == null)
                return;

            if (songGridView.InvokeRequired)
                songGridView.Invoke(new MethodInvoker(delegate { UpdateShowingPlaylist(); }));
            else
            {
                songGridView.DataSource = null;
                songGridView.DataSource = new List<Song>(showingPlaylist.GetSongs());

                if (showingPlaylist == shuffleQueue)
                    SetShuffleQueueColors();
            }

            if (statusStrip.Items.Count > 0)
                UpdateStatusStrip();

            if (showingPlaylist == shuffleQueue)
                songGridView.ClearSelection();

            SetSortGlyph();
        }

        private void SetShuffleQueueColors()
        {
            foreach (DataGridViewRow row in songGridView.Rows)
            {
                if (row.Index == shuffleQueue.CurrentIndex)
                    row.DefaultCellStyle.BackColor = SystemColors.GradientInactiveCaption;
                else if (row.Index % 2 == 0)
                    row.DefaultCellStyle.BackColor = Color.White;
                else
                    row.DefaultCellStyle.BackColor = SystemColors.Control;
            }
        }

        private void SetSortGlyph()
        {
            foreach (DataGridViewColumn column in songGridView.Columns)
                column.HeaderCell.SortGlyphDirection = SortOrder.None;
            if (showingPlaylist.SortColumnIndex >= 0)
                songGridView.Columns[showingPlaylist.SortColumnIndex].HeaderCell.SortGlyphDirection = showingPlaylist.SortOrder;
        }

        private void LoadPlaylists()
        {
            playlists = new List<PlaylistBase>();
            playlists.Add(library);

            if (!Directory.Exists(PlaylistDirectoryPath))
                Directory.CreateDirectory(PlaylistDirectoryPath);

            // Load shuffle queue
            shuffleQueue = PlaylistFactory.MakePlaylist(ShuffleQueuePath, library, settings) as ShuffleQueue;
            if (shuffleQueue == null)
            {
                shuffleQueue = new ShuffleQueue(library, settings);
                shuffleQueue.Save();
            }
            playlists.Add(shuffleQueue);

            // Load other playlists
            string[] playlistFiles = Directory.GetFiles(PlaylistDirectoryPath, "*.pl", SearchOption.AllDirectories);
            foreach (string playlistPath in playlistFiles)
            {
                if (playlistPath == ShuffleQueuePath)
                    continue;

                var pl = PlaylistFactory.MakePlaylist(playlistPath, library, settings);
                if (pl != null)
                    playlists.Add(pl);
            }
        }

        private void SetPlaylistGridView()
        {
            SortPlaylists();
            playlistGridView.DataSource = null;
            playlistGridView.Columns.Clear();
            playlistGridView.DataSource = playlists;
            playlistGridView.ClearSelection();

            int selectedIndex = playlists.IndexOf(showingPlaylist);
            playlistGridView.Rows[selectedIndex].Cells[0].Selected = true;
        }

        private void SortPlaylists()
        {
            if (playlists.Count <= 2)
                return;

            var toSort = playlists.Skip(2).ToList();

            toSort.Sort((pl1, pl2) => pl1.Name.CompareTo(pl2.Name));

            playlists.RemoveRange(2, toSort.Count);
            playlists.AddRange(toSort);
        }

        private PlaylistBase GetPlaylist(string name)
        {
            foreach (PlaylistBase pl in playlists)
                if (pl.Name.ToLower() == name.ToLower())
                    return pl;
            return null;
        }

        private void ShowSongInPlaylist(Song song, PlaylistBase playlist)
        {
            if (!playlist.GetSongs().Contains(song))
                return;

            if (showingPlaylist != playlist)
                ChangeToPlaylist(playlist);

            foreach (DataGridViewRow row in songGridView.Rows)
            {
                if (row.DataBoundItem == song)
                {
                    var rowIndex = row.Index;
                    songGridView.ClearSelection();
                    songGridView.Rows[rowIndex].Selected = true;
                    songGridView.FirstDisplayedScrollingRowIndex = rowIndex;
                    break;
                }
            }
        }

        #region Playlist manipulatiom methods
        private void DeletePlaylist(DataGridViewCell cell)
        {
            bool deleted = false;
            PlaylistBase pl = playlists[cell.RowIndex];
            if (pl != library && pl != shuffleQueue)
            {
                if (pl == showingPlaylist)
                    ChangeToPlaylist(library);
                if (pl == shuffleQueue.Source)
                {
                    shuffleQueue.Source = library;
                    settings.Shufflequeue_SourcePlaylistName = library.Name;
                }
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
            renamingPlaylist = GetPlaylist(playlistGridView.CurrentCell.Value.ToString()) as Playlist;
            playlistGridView.BeginEdit(true);
        }

        private void playlistGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (renamingPlaylist == null)
                return;

            string currentName = e.FormattedValue.ToString();

            string oldName = this.renamingPlaylist.Name;
            string oldPath = this.renamingPlaylist.Path;
            
            if (string.IsNullOrEmpty(currentName))
                e.Cancel = true;
            else
            {
                foreach (PlaylistBase pl in playlists)
                {
                    if (currentName.ToLower() == pl.Name.ToLower())
                        if (currentName.ToLower() != oldName.ToLower())
                        {
                            e.Cancel = true;
                            break;
                        }
                }
            }

            // save change file name as well
            if (!e.Cancel)
            {
                if (currentName.ToLower() != oldName.ToLower())
                {
                    renamingPlaylist.Name = currentName;
                    try
                    {
                        if (File.Exists(oldPath))
                            System.IO.File.Move(oldPath, renamingPlaylist.Path);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Playlist rename exception: " + ex.Message);
                    }
                }

                renamingPlaylist = null;
            }
            
        }
        
        #endregion

        #region Playlist selection
        private void playlistGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ChangeToPlaylist(playlists[e.RowIndex]);
        }

        private void ChangeToPlaylist(PlaylistBase playlist)
        {
            if (showingPlaylist == playlist)
                return;

            if (playlist != library)
                ResetSearchBox();
            showingPlaylist = playlist;

            if (showingPlaylist == shuffleQueue)
                shuffleQueue.Populate();

            UpdateShowingPlaylist();
            songGridView.ClearSelection();

            SetPlaylistGridView();
            UpdateStatusStrip();
        }
        #endregion

        #endregion

        #region Settings window
        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hook.Dispose();
            var settingsWindow = new SettingsWindow(this, this.playlists);
            settingsWindow.StartPosition = FormStartPosition.CenterParent;
            settings.Save(SettingsPath);
            
            if (settingsWindow.ShowDialog() == DialogResult.OK)
                SetSettings();
            else
                settings = Settings.Load(SettingsPath);
            SetUpGlobalHotkeys();
        }

        private void SetSettings()
        {
            SetSongGridViewStyle();

            shuffleQueue.Source = GetPlaylist(settings.Shufflequeue_SourcePlaylistName);
            shuffleQueue.Populate();

            UpdateShowingPlaylist();
            ResetSearchBox();
            RefreshSongGridView();
            songGridView.Focus();

            settings.Save(SettingsPath);
        }

        private void SetStartupSetting()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(startupRegKey, true);
            
            if (settings.RunAtStartup)
                key.SetValue(this.Text, Path.Combine(Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location), "KoPlayer.exe"));
            else
                if (key.GetValue(this.Text) != null)
                    key.DeleteValue(this.Text);
            key.Close();
        }
        #endregion

        #region Exit
        private void OnExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop playback & timers
            searchBarTimer.Stop();
            searchBarTimer.Dispose();
            searchLibraryTimer?.Stop();
            searchLibraryTimer?.Dispose();
            saveTimer.Stop();
            saveTimer.Dispose();

            trayIcon.Dispose();
            musicPlayer.Dispose();

            // Save settings
            SaveFormDimensions();

            settings.StartupPlaylist = showingPlaylist.Name;
            if (settings.StartupPlaylist == "Search Results")
                settings.StartupPlaylist = library.Name;

            settings.Volume = volumeTrackBar.Value;
            settings.DeviceVolume = musicPlayer.DeviceVolume;

            settings.Save(SettingsPath);

            // Save data grid column settings
            columnSettings = new ColumnSettings(songGridView.Columns);
            columnSettings.Save(ColumnSettingsPath);

            // Saving of playlists is done after the form has closed (program.cs)
        }

        private void SaveFormDimensions()
        {
            Show();
            WindowState = FormWindowState.Normal;
            settings.FormWidth = this.Width;
            settings.FormHeight = this.Height;
            Hide();
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
                    UpdateShowingPlaylist();
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
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add("Adding files...");
            progressBar = new ToolStripProgressBar("add_progress");
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            statusStrip.Items.Add(progressBar);
            UpdateStatusStrip();
            library.ReportProgress += library_ReportProgress;
        }

        private void library_LibraryChanged(object sender, EventArgs e)
        {
            if (showingPlaylist == shuffleQueue)
                shuffleQueue.Populate();

            UpdateShowingPlaylist();

            UpdateStatusStrip();
        }

        private void library_ReportProgress(object sender, ReportProgressEventArgs e)
        {
            if (e.ProgressPercentage < 100)
                this.progressBar.Value = (int)(e.ProgressPercentage);
            else if (e.ProgressPercentage == 100)
            {
                statusStrip.Items.Clear();
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

        private void UpdateStatusStrip()
        {
            if (statusStrip.InvokeRequired)
                statusStrip.Invoke(new MethodInvoker(delegate { UpdateStatusStrip(); }));
            else
            {
                statusStrip.Items["playlist_info"].Text = showingPlaylist.ToString();

                string itemText = "";
                if (musicPlayer.PlaybackState == PlaybackState.Playing)
                    itemText = "Playing";
                else if (musicPlayer.PlaybackState == PlaybackState.Paused)
                    itemText = "Paused";
                statusStrip.Items["playback_status"].Text = itemText;

                statusStrip.Items["playlist_info_separator"].Visible = !(itemText.Length == 0);
            }
        }

        private void UpdateTrayIconText()
        {
            string text = "";
            if (musicPlayer.PlaybackState != PlaybackState.Stopped && currentlyPlaying != null)
            {
                // Upper limit is 128 chars, divided by 3 rounded down = 42
                int limit = 42;
                text += ShortenString(currentlyPlaying.Title, limit) + "\n";
                text += ShortenString(currentlyPlaying.Artist, limit) + "\n";
                text += ShortenString(currentlyPlaying.Album, limit);
            }
            else
                text = this.Text;
            SetNotifyIconText(trayIcon, text);
        }

        /// <summary>
        /// Ups limit on tray icon text from 63 to 127 chars. Found at stackoverflow
        /// </summary>
        /// <param name="ni"></param>
        /// <param name="text"></param>
        public static void SetNotifyIconText(NotifyIcon ni, string text)
        {
            if (text.Length >= 128) throw new ArgumentOutOfRangeException("Text limited to 127 characters");
            Type t = typeof(NotifyIcon);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("text", hidden).SetValue(ni, text);
            if ((bool)t.GetField("added", hidden).GetValue(ni))
                t.GetMethod("UpdateIcon", hidden).Invoke(ni, new object[] { true });
        }

        private void SetTrayIconContextMenu()
        {
            ContextMenu cm = new ContextMenu();
            if (currentlyPlaying != null)
            {
                string itemText = "Play";
                if (musicPlayer.PlaybackState == PlaybackState.Playing)
                {
                    itemText = "Pause";

                    var ratingMenu = new MenuItem("Rate current song");
                    for (int i = 0; i <= 5; i++)
                    {
                        int rating = i;
                        ratingMenu.MenuItems.Add("Rate " + rating);
                        ratingMenu.MenuItems[i].Click += (o, e) => { RateSong(currentlyPlaying, rating); };
                    }
                    cm.MenuItems.Add(ratingMenu);
                }

                cm.MenuItems.Add(CreateMenuItem(itemText, playpauseButton_Click));

                cm.MenuItems.Add(CreateMenuItem("Play next", nextButton_Click));
                cm.MenuItems.Add(CreateMenuItem("Play previous", previousButton_Click));
            }
            cm.MenuItems.Add(CreateMenuItem("Show KoPlayer", showKoPlayer_Event));
            cm.MenuItems.Add(CreateMenuItem("Close", OnExitButton_Click));
            trayIcon.ContextMenu = cm;
        }

        private void showKoPlayer_Event(object sender, EventArgs e)
        {
            ShowKoPlayer();
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
            if (albumArtBox.InvokeRequired)
                albumArtBox.Invoke(new MethodInvoker(delegate { UpdateSongImage(); }));
            else
                albumArtBox.Image = currentAlbumArt;
        }

        private void UpdateSongInfoLabel()
        {
            if (songInfoLabel.InvokeRequired)
                songInfoLabel.Invoke(new MethodInvoker(delegate { UpdateSongInfoLabel(); }));
            else
            {
                if (musicPlayer.PlaybackState != PlaybackState.Stopped)
                {
                    int albumCharLimit = 60;
                    string text =
                          currentlyPlaying.Title + "\n"
                        + currentlyPlaying.Artist + "\n";
                    if (currentlyPlaying.Album.Length > albumCharLimit)
                        text += new string(currentlyPlaying.Album.Take(albumCharLimit).ToArray()) + "...";
                    else
                        text += currentlyPlaying.Album;
                    songInfoLabel.Text = text;
                }
                else
                    songInfoLabel.Text = "";
            }
        }

        private void UpdatePlayPauseButtonImage()
        {
            if (musicPlayer.PlaybackState == PlaybackState.Playing)
                playpauseButton.ImageList = pauseButton_imageList;
            else
                playpauseButton.ImageList = playButton_imageList;
        }

        private void SelectCurrentPlaylist()
        {
            playlistGridView.ClearSelection();
            foreach (DataGridViewRow row in playlistGridView.Rows)
            {
                if (row.DataBoundItem == showingPlaylist)
                {
                    playlistGridView.CurrentCell = row.Cells[0];
                    return;
                }
            }
        }

        #endregion 

        #region Playback control
        private void PlaySong(Song song, PlaylistBase inPlaylist)
        {
            searchBarTimer.Stop();
            musicPlayer.Stop();
            musicPlayer.Volume = 0;

            if (MusicPlayer.PlaybackDevice.FriendlyName != settings.DeviceName)
                musicPlayer.SetDevice(settings.DeviceName);

            // Check Last.fm scrobbling requirements
            if (currentSongTimePlayed.Ticks > 0.8 * musicPlayer.Length.Ticks ||
                currentSongTimePlayed.TotalMinutes > 4)
            {
                currentSongTimePlayed = TimeSpan.Zero;
                currentlyPlaying.LastPlayed = DateTime.Now;
                currentlyPlaying.PlayCount++;

                if (settings.ScrobblingEnabled)
                    if (currentlyPlaying.Length.TotalSeconds > 30)
                        lfmHandler.ScrobbleSong(currentlyPlaying);

                RefreshSongGridView();
            }

            if (!ReloadSong(song))
                return;

            ResetSearchBarTimer();
            searchBarTimer.Stop();

            try
            {
                 musicPlayer.Open(song.Path);
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
            {
                shuffleQueue.Populate();

                if (showingPlaylist == shuffleQueue)
                    UpdateShowingPlaylist();
            }

            // Saves changed from tag editing when song is not playing any more
            if (songToSave != null &&
                songToSave.Path != currentlyPlaying.Path)
            {
                songToSave.Save();
                songToSave = null;
            }

            // Show song popup according to settings
            if (settings.PopupOnSongChange)
                ShowCurrentSongPopup();
            
            if (settings.SaveCurrentlyPlayingToFile)
                SetNowPlaying(song);

            inPlaylist.Save();
        }

        void musicPlayer_ShouldPlay(object sender, EventArgs e)
        {
            PlayMusic();
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
                this.currentAlbumArt = song.ReloadAndGetImage();
                UpdateSong(song);
            }
            catch (SongReadException ex)
            {
                OnSongReadException(ex, song);
                return false;
            }
            return true;
        }

        private void OnSongReadException(SongReadException ex, Song song)
        {
            MessageBox.Show("Song at " + song.Path +
                " could not be read and will be removed from the library.");
            library.Remove(song);
        }

        private void PlayMusic()
        {
            musicPlayer.Play();
            UpdateSearchBar(musicPlayer.Length, musicPlayer.Position);
            searchBarTimer.Start();
            UpdateStatusStrip();
            UpdatePlayPauseButtonImage();
            
            musicPlayer.Volume = volumeTrackBar.Value;
        }

        private void PauseMusic()
        {
            musicPlayer.Volume = 0;
            musicPlayer.Pause();
            searchBarTimer.Stop();
            UpdateStatusStrip();
            UpdatePlayPauseButtonImage();
        }

        private void StopPlaying()
        {
            albumArtBox.Image = null;
            searchBarTimer.Stop();
            musicPlayer.Stop();
            musicPlayer.Volume = 0;
            UpdateSongInfoLabel();
            UpdateStatusStrip();
            UpdatePlayPauseButtonImage();
        }

        private void PlayNextSong()
        {
            PlaySong(playingPlaylist.GetNext(), playingPlaylist);
        }

        private void PlayPreviousSong()
        {
            PlaySong(playingPlaylist.GetPrevious(), playingPlaylist);
        }

        private void PlayOrPause()
        {
            if (musicPlayer.PlaybackState == PlaybackState.Paused)
                PlayMusic();
            else if (musicPlayer.PlaybackState == PlaybackState.Playing)
                PauseMusic();
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

        #endregion

        #region Playback timer
        private void ResetSearchBarTimer()
        {
            if (searchBarTimer != null)
                searchBarTimer.Dispose();
            searchBarTimer = new System.Timers.Timer(searchBarTimerInterval);
            searchBarTimer.Elapsed += searchBarTimer_Elapsed;
            SetSearchBarValue(0);
            currentSongTimePlayed = TimeSpan.Zero;
            oldPosition = TimeSpan.Zero;
        }

        private void searchBarTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan length = currentlyPlaying.Length;
            TimeSpan position = musicPlayer.Position;
            
            UpdateSearchBar(length, position);
            SetCurrentTimeLabel(position);

            if (Math.Abs(position.Seconds - oldPosition.Seconds) < 5)
                currentSongTimePlayed += position - oldPosition;

            oldPosition = position;

            if (searchBarTimer.Enabled)
                if (musicPlayer.PlaybackState == PlaybackState.Stopped || position > length)
                    PlayNextSong();
        }
        
        #endregion

        #region Hotkeys
        #region Local Hotkeys
        private void songGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (!songGridView.Focused)
                return;

            if (e.KeyCode == Keys.Enter)
            {
                if (songGridView.SelectedRows.Count == 1)
                    PlaySong(songGridView.SelectedRows[0].DataBoundItem as Song, showingPlaylist);
            }
            else if (e.KeyCode == Keys.Delete)
                DeleteSongs(songGridView.SelectedRows);
            else if (e.Control && e.KeyCode == Keys.I)
            {
                e.Handled = true;
                ShowSongListProperties(new List<Song> { songGridView.SelectedRows[0].DataBoundItem as Song });
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                searchBox.Focus();
                searchBox.SelectAll();
                return;
            }

            if (musicPlayer.PlaybackState != PlaybackState.Playing)
                return;

            if (e.Control)
                if (settings.RatingHotkeys.Contains(e.KeyCode))
                {
                    RateSong(currentlyPlaying, Array.IndexOf(settings.RatingHotkeys, e.KeyCode));
                    RefreshSongGridView();
                }

            if (!searchBox.Focused && !volumeTrackBar.Focused)
            {
                if (e.KeyCode == Keys.Right)
                    PlayNextSong();
                else if (e.KeyCode == Keys.Left)
                    PlayPreviousSong();
            }
        }

        private void RateSongs(DataGridViewSelectedRowCollection rows, int rating)
        {
            List<Song> songs = new List<Song>();
            foreach (DataGridViewRow row in rows)
            {
                Song s = row.DataBoundItem as Song;
                s.Rating = rating;
                songs.Add(s);
            }

            library.ResetSearchDictionaries();

            foreach (Song s in songs)
                UpdateSong(s);

            RefreshSongGridView();
            if (showingPlaylist.GetType() == typeof(FilterPlaylist))
                UpdateShowingPlaylist();
        }

        private void RateSong(Song s, int rating)
        {
            s.Rating = rating;
            library.UpdateSongInfo(currentlyPlaying);
            UpdateSong(s);
            RefreshSongGridView();
            if (showingPlaylist.GetType() == typeof(FilterPlaylist))
                UpdateShowingPlaylist();
        }

        private void UpdateSong(Song song)
        {
            foreach (PlaylistBase pl in playlists)
                pl.UpdateSongInfo(song);
        }

        private void DeleteSongs(DataGridViewSelectedRowCollection rows)
        {
            if (showingPlaylist.GetType() == typeof(FilterPlaylist))
                return;

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
                    return;
            }

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
                            if (playingPlaylist.CurrentIndex == i)
                                StopPlaying();
                    }
                }
                showingPlaylist.Remove(indexList);
            }

            if (showingPlaylist == shuffleQueue)
                shuffleQueue.Populate();
            
            showingPlaylist.Save();

            UpdateShowingPlaylist();
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

        private List<DataGridViewRow> GetSortedRowList(DataGridViewSelectedRowCollection rows)
        {
            List<DataGridViewRow> ret = new List<DataGridViewRow>();

            foreach (DataGridViewRow row in rows)
                ret.Add(row);

            ret.Sort((r1, r2) => r2.Index.CompareTo(r1.Index));

            return ret;
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
            foreach (var hk in settings.GlobalHotkeys)
                if (e.Modifier == hk.Modifier && e.Key == hk.Key)
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
            volumeTrackBar.SetValue(volumeTrackBar.Value + volumeTrackBar.SmallChange);
        }

        private void DecreaseVolume()
        {
            volumeTrackBar.SetValue(volumeTrackBar.Value - volumeTrackBar.SmallChange);
        }

        private void ShowCurrentSongPopup()
        {
            if (currentlyPlaying != null)
                SongInfoPopup.ShowPopup(currentlyPlaying, currentAlbumArt, songInfoPopupTime);
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

        private void SetCurrentTimeLabel(TimeSpan time)
        {
            if (currentTime_Label.InvokeRequired)
                currentTime_Label.Invoke(new MethodInvoker(delegate { SetCurrentTimeLabel(time); }));
            else
            {
                if (!currentTime_Label.IsDisposed)
                    currentTime_Label.Text = time.ToString(Song.LengthFormat);
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
        #endregion

        #region Searchbar events
        private void searchBar_ValueChanged(object sender, EventArgs e)
        {
            if (!stopSearchBarUpdate)
                return;

            double perc = searchBar.Value / (double)searchBar.Maximum;
            TimeSpan position = TimeSpan.FromMilliseconds(musicPlayer.Length.TotalMilliseconds * perc);
            musicPlayer.Position = position;
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
            MessageBox.Show(aboutMessage);
        }
        #endregion

        #region Sorting
        private void songGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (showingPlaylist != shuffleQueue)
                {
                    showingPlaylist.Sort(e.ColumnIndex, songGridView.Columns[e.ColumnIndex].HeaderText);
                    UpdateShowingPlaylist();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                ContextMenu cm = new ContextMenu();
                foreach (DataGridViewColumn column in songGridView.Columns)
                {
                    var item = new MenuItem(column.HeaderText);
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
                UpdateShowingPlaylist();
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
            this.shouldSearch = false;
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            if (searchBox.Text == searchBoxDefault)
                return;

            if (shouldSearch || searchBox.Text != "")
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
                this.shouldSearch = true;
            }
        }

        private void searchLibraryTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
            returnPlaylist = showingPlaylist;
            ChangeToPlaylist(library);
            if (searchBox.Text.Length > 0)
            {
                showingPlaylist = library.Search(searchBox.Text);
            }
            else if (searchBox.Text.Length == 0)
                library.ResetSearchDictionaries();

            UpdateShowingPlaylist();
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
            {
                songGridView.Focus();
                if (e.KeyCode == Keys.Escape)
                {
                    ResetSearchBox();
                    showingPlaylist = returnPlaylist ?? library;
                    SelectCurrentPlaylist();
                    UpdateShowingPlaylist();
                }
            }
        }
        #endregion
        #region SaveNowPlayingToFile

        private void SetNowPlaying(Song song)
        {
            try
            {
                var nowPlayingText = GetNowPlayingString(song);
                File.WriteAllText(NowPlayingPath, nowPlayingText, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);
            }
            
        }

        private string GetNowPlayingString(Song song)
        {
            return settings.CurrentlyPlayingBaseString
                .Replace("%title%", song.Title)
                .Replace("%artist%", song.Artist)
                .Replace("%album%", song.Album) + "\n";
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

            var newPlaylist = new Playlist(library, name);
            playlists.Add(newPlaylist);
            SetPlaylistGridView();
            ChangeToPlaylist(newPlaylist);
        }

        private string GetNewPlaylistName(bool filter)
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

                foreach (PlaylistBase pl in playlists)
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
            ShowCreateNewFilterPlaylistPopup();
        }

        private void ShowCreateNewFilterPlaylistPopup()
        {
            FilterPlaylistWindow popup = new FilterPlaylistWindow();
            popup.SetStartPosition();

            DialogResult dr = popup.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                var filters = popup.GetFilterList();
                var newFpl = new FilterPlaylist(library, GetNewPlaylistName(true), filters);
                playlists.Add(newFpl);
                SetPlaylistGridView();
                ChangeToPlaylist(newFpl);
            }
        }
        #endregion

        #region Drag & Drop songs
        private void songGridView_RowDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DataGridViewSelectedRowCollection selectedRows = (DataGridViewSelectedRowCollection)e.Item;
                List<DataGridViewRow> sortedRows = GetSortedRowList(selectedRows);

                //Create dataobject and do dragdrop
                DragDropSongs data = new DragDropSongs(DataFormats.FileDrop, sortedRows);
                songGridView.DoDragDrop(data, DragDropEffects.Copy);
            }
        }

        private void songGridView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.Data.GetType() == typeof(DragDropSongs))
                {
                    if (showingPlaylist.GetType() == typeof(Playlist) ||
                        showingPlaylist.GetType() == typeof(ShuffleQueue))
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
            var p = songGridView.PointToClient(new Point(e.X, e.Y));
            var info = songGridView.HitTest(p.X, p.Y);

            if (info.RowIndex < 0)
                return;
                var pl = showingPlaylist;

            if (pl == library)
                return;

            // Get datagridview rows from data
            var data = (DragDropSongs)e.Data;
            data.ReturnPaths = false;
            var rows = (List<DataGridViewRow>)
                data.GetData(DataFormats.FileDrop);

            var playlist = pl as Playlist;

            var songs = new List<Song>();
            
            int originalIndex = -1;
            if (showingPlaylist == shuffleQueue)
                originalIndex = shuffleQueue.CurrentIndex;
  
            foreach (var row in rows)
            {
                songs.Add((Song)row.DataBoundItem);
                pl.Remove(row.Index);

                if (playingPlaylist == shuffleQueue &&
                    showingPlaylist == playingPlaylist &&
                    row.Index == shuffleQueue.CurrentIndex)
                    shuffleQueue.CurrentIndex = info.RowIndex;
            }

            songs.Reverse();

            if (originalIndex != -1 && info.RowIndex == originalIndex)
                playlist.Insert(info.RowIndex + 1, songs);
            else
                playlist.Insert(info.RowIndex, songs);

            if (showingPlaylist == shuffleQueue)
                shuffleQueue.Populate();
                    
            UpdateShowingPlaylist();
        }

        private void HandleFileDragDrop(DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (CanAddPaths(paths))
            {
                var songPaths = new List<string>();
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

        /// <summary>
        /// True if paths are directories or files of acceptable extensions
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
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
            var info = playlistGridView.HitTest(p.X, p.Y);

            e.Effect = DragDropEffects.None;

            if (info.RowIndex >= 0)
            {
                PlaylistBase pl = playlists[info.RowIndex];

                if (pl.GetType() == typeof(Playlist) ||
                    pl.GetType() == typeof(ShuffleQueue))
                    e.Effect = DragDropEffects.Copy;
            }
        }

        private void playlistGridView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetType() == typeof(DragDropSongs))
            {
                var data = (DragDropSongs)e.Data;
                data.ReturnPaths = false;

                var p = playlistGridView.PointToClient(new Point(e.X, e.Y));
                var info = playlistGridView.HitTest(p.X, p.Y);

                if (info.RowIndex >= 0)
                {

                    var rows = (List<DataGridViewRow>)data.GetData(DataFormats.FileDrop);
                    //Rows recieved are reverse order from what we want here
                    rows.Reverse();

                    var songs = new List<Song>();
                    foreach (DataGridViewRow row in rows)
                        songs.Add((Song)row.DataBoundItem);

                    var pl = GetPlaylist(playlistGridView.Rows[info.RowIndex].Cells[0].Value.ToString());
                    if (pl.GetType() == typeof(Playlist) ||
                    pl.GetType() == typeof(ShuffleQueue))
                    {
                        pl.Add(songs);

                        if (pl == showingPlaylist)
                            UpdateShowingPlaylist();
                    }
                }
            }
        }
        #endregion

        #region Minimize to tray

        private void showKoPlayerToggleShow_Event(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
                HideKoPlayer();
            else
                ShowKoPlayer();
        }

        public void ShowKoPlayer()
        {
            this.TopMost = true;
            this.Show();
            this.WindowState = FormWindowState.Normal;
            //Updates stops flickering when window de-minimizes
            this.nextButton.Update();
            this.playpauseButton.Update();
            this.previousButton.Update();
            this.songInfoLabel.Update();
            this.playlistGridView.Update();
            this.TopMost = false;
            this.songGridView.Focus();
        }

        private void HideKoPlayer()
        {
            trayIcon.Visible = true;
            if (settings.MinimizeToTray)
            {
                this.Hide();
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                HideKoPlayer();
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
                cm.MenuItems.Add(CreateMenuItem("New filter playlist",
                    newRatingFilterPlaylistToolStripMenuItem_Click));

                clickedPlaylistIndex = playlistGridView.HitTest(e.X, e.Y).RowIndex;

                if (clickedPlaylistIndex >= 0)
                {
                    playlistGridView.ClearSelection();
                    playlistGridView.Rows[clickedPlaylistIndex].Selected = true;
                    playlistGridView.Rows[clickedPlaylistIndex].Cells[0].Selected = true;

                    if (clickedPlaylistIndex > 1)
                    {
                        if (playlists[clickedPlaylistIndex].GetType() == typeof(FilterPlaylist))
                            cm.MenuItems.Add("Edit filter playlist", editFilterPlaylist);
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

        private void editFilterPlaylist(object sender, EventArgs e)
        {
            var clickedPlaylist = playlists[clickedPlaylistIndex] as FilterPlaylist;
            if (clickedPlaylist != null)
            {
                FilterPlaylistWindow popup = new FilterPlaylistWindow(clickedPlaylist);
                popup.SetStartPosition();

                DialogResult dr = popup.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    clickedPlaylist.Filters = popup.GetFilterList();
                    clickedPlaylist.FilterLibrary();
                    
                    if (showingPlaylist != clickedPlaylist)
                        ChangeToPlaylist(clickedPlaylist);
                    else
                        UpdateShowingPlaylist();
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
            for (int i = 0; i <= 5; i++)
            {
                int rating = i;
                ratingMenu.MenuItems.Add("Rate " + rating);
                ratingMenu.MenuItems[i].Click += (o, e) => { RateSongs(songGridView.SelectedRows, rating); };
            }
            cm.MenuItems.Add(ratingMenu);

            cm.MenuItems.Add(CreateMenuItem("Show file in explorer", songGridViewRightClickShowExplorer));
            if (showingPlaylist.GetType() != typeof(FilterPlaylist))
                cm.MenuItems.Add(CreateMenuItem("Delete", songGridViewRightClickDelete));
            cm.MenuItems.Add(CreateMenuItem("Properties", songGridViewRightClickProperties));
            return cm;
        }

        #region Right click menu events

        private void songGridViewRightClickPlay(object sender, EventArgs e)
        {
            PlaySong((Song)songGridView.Rows[clickedSongIndex].DataBoundItem, showingPlaylist);
        }

        private void songGridViewRightClickAddToShuffleQueueNext(object sender, EventArgs e)
        {
            List<Song> songs = new List<Song>();

            List<DataGridViewRow> sortedRows = GetSortedRowList(songGridView.SelectedRows);

            foreach (DataGridViewRow row in sortedRows)
                songs.Add((Song)row.DataBoundItem);

            songs.Reverse();

            shuffleQueue.Insert(shuffleQueue.CurrentIndex + 1, songs);

            if (showingPlaylist == shuffleQueue)
            {
                UpdateShowingPlaylist();
            }
        }

        private void songGridViewRightClickAddToShuffleQueueBottom(object sender, EventArgs e)
        {
            AddToShuffleQueueBottom(songGridView.SelectedRows);
        }

        private void AddToShuffleQueueBottom(DataGridViewSelectedRowCollection rows)
        {
            List<DataGridViewRow> sortedRows = GetSortedRowList(rows);
            sortedRows.Reverse();

            foreach (DataGridViewRow row in sortedRows)
                shuffleQueue.Add((Song)row.DataBoundItem);

            if (showingPlaylist == shuffleQueue)
                UpdateShowingPlaylist();
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
                    s.ReloadTags();
                }
                catch (SongReadException ex)
                {
                    OnSongReadException(ex, s);
                    exists = false;
                }
            }

            if (!exists || songs.Count == 0)
                return;
            
            if (songs.Count > 1)
                OpenMultiSongPropertiesWindow(songs);
            else
                OpenSongPropertiesWindow(songs.First());
        }

        private void OpenMultiSongPropertiesWindow(List<Song> songs)
        {
            var multiSongWindow = new MultiSongPropertiesWindow(this, songs, showingPlaylist, library);
            multiSongWindow.SavePlayingSong += popup_SavePlayingSong;
            multiSongWindow.SongChanged += songPropertiesWindow_SongChanged;
            
            multiSongWindow.StartPosition = FormStartPosition.CenterParent;
            multiSongWindow.ShowDialog();

            ShowSongInPlaylist(songs.First(), showingPlaylist);
        }

        private void OpenSongPropertiesWindow(Song song)
        {
            var singleSongWindow = new SongPropertiesWindow(this, song, clickedSongIndex, showingPlaylist, library);
            singleSongWindow.SavePlayingSong += popup_SavePlayingSong;
            singleSongWindow.SongChanged += songPropertiesWindow_SongChanged;

            singleSongWindow.StartPosition = FormStartPosition.CenterParent;
            singleSongWindow.ShowDialog();

            ShowSongInPlaylist(singleSongWindow.Song, showingPlaylist);
        }

        private void songPropertiesWindow_SongChanged(object sender, SongChangedEventArgs e)
        {
            UpdateSong(e.ChangedSong);
            UpdateShowingPlaylist();
        }

        private void popup_SavePlayingSong(object sender, SavePlayingSongEventArgs e)
        {
            songToSave = e.SavingSong;
        }
        #endregion

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

        private void exportPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var exportDirectory = dialog.SelectedPath;
                var info = new DirectoryInfo(exportDirectory);

                foreach (var song in showingPlaylist.GetSongs())
                {
                    var fileName = song.Path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Last();

                    var counter = 0;

                    string GetFilePath(string newDir, string file, int i)
                    {
                        if (i == 0)
                        {
                            return $"{newDir}{Path.DirectorySeparatorChar}{file}";
                        }

                        var splitFile = file.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        var extension = splitFile.Last();

                        var newFileName = splitFile.Take(splitFile.Length - 1).Aggregate((p1, p2) => $"{p1}.{p2}") + $" ({i})." + extension;

                        return $"{newDir}{Path.DirectorySeparatorChar}{newFileName}";
                    }

                    var newPath = GetFilePath(exportDirectory, fileName, counter);

                    while (File.Exists(newPath))
                    {
                        newPath = GetFilePath(exportDirectory, fileName, ++counter);
                    }
                    
                    File.Copy(song.Path, newPath);
                }
            }
        }
    }
}
