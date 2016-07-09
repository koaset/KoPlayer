using KoPlayer.Lib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using KoScrobbler.Entities;
using KoScrobbler.Interfaces;
using KoScrobbler;
using System.Linq;

namespace KoPlayer
{
    public class LastfmHandler
    {
        private const string sessionRegistryPath = "Software\\KoPlayer";
        private const string apiKey = "4eedd270b2824403af15f9a81407f7ce";
        private const string apiSecret = "963517814b9653e6c9eaeab075f0d336";

        public event EventHandler StatusChanged;

        public string Status { get; set; }

        public string UserName { get; private set; }

        private BackgroundWorker scrobbleWorker;
        private List<Scrobble> scrobbleQueue;
        private ILastFmScrobbler scrobbler;

        public LastfmHandler()
        {
            scrobbler = new Scrobbler(apiKey, apiSecret);

            LoadSession();

            Initialize();
        }

        public void Initialize()
        {
            Status = "Not Connected";

            scrobbleWorker = new BackgroundWorker();
            scrobbleWorker.WorkerSupportsCancellation = false;
            scrobbleWorker.WorkerReportsProgress = false;
            scrobbleWorker.DoWork += scrobbleWorker_DoWork;

            scrobbleQueue = new List<Scrobble>();
        }

        public async void ResumeSessionAsync()
        {
            if (string.IsNullOrEmpty(scrobbler.SessionKey) ||
                string.IsNullOrEmpty(UserName))
                return;

            ChangeStatus("Resuming");

            try
            {
                var response = await scrobbler.ValidateSessionAsync(UserName, scrobbler.SessionKey);

                if (response.Success)
                    ChangeStatus("Connected");
                else
                    ChangeStatus("Not Connected");
            }
            catch
            {
                ChangeStatus("Not Connected");
            }
        }

        public async void LoginAsync(string userName, string password)
        {
            ChangeStatus("Connecting");

            UserName = userName;

            try
            {
                var response = await scrobbler.CreateSessionAsync(userName, password);

                if (response.Success)
                {
                    ChangeStatus("Connected");
                    scrobbler.SessionKey = response.SessionKey;
                    SaveSession();
                }
                else
                    ChangeStatus("Not connected");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("An error occured: " + ex.ToString());
                ChangeStatus("Connection problem");
            }
        }

        public void ScrobbleSong(Song song)
        {
            scrobbleQueue.Add(new Scrobble(song.Artist, song.Album, song.Title, song.LastPlayed));
            StartWorker();
        }

        private void StartWorker()
        {
            if (!scrobbleWorker.IsBusy)
                scrobbleWorker.RunWorkerAsync();
        }

        private void scrobbleWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ScrobbleAsync();
        }

        public async void ScrobbleAsync()
        {
            if (scrobbleQueue.Count == 0)
                return;

            // Only allowed 50 songs in each scrobble
            var toScrobble = scrobbleQueue.Take(50).ToList();

            try
            {
                var response = await scrobbler.ScrobbleAsync(toScrobble);
                
                if (response.Success)
                    scrobbleQueue.RemoveRange(0, toScrobble.Count);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);
            }
        }

        private void ChangeStatus(string newStatus)
        {
            Status = newStatus;

            if (Status == "Connected" && scrobbleQueue.Count > 0)
                StartWorker();

            StatusChanged?.Invoke(this, new EventArgs());
        }

        private void SaveSession()
        {
            var key = Registry.CurrentUser.OpenSubKey(sessionRegistryPath, true) ?? Registry.CurrentUser.CreateSubKey(sessionRegistryPath);
            key.Close();

            using (var usernameKey = Registry.CurrentUser.OpenSubKey(sessionRegistryPath, true))
            {
                usernameKey.SetValue("username", UserName);
            }

            using (var sessionKey = Registry.CurrentUser.OpenSubKey(sessionRegistryPath, true))
            {
                sessionKey.SetValue("sessionKey", scrobbler.SessionKey);
            }
        }

        private void LoadSession()
        {
            using (var usernameKey = Registry.CurrentUser.OpenSubKey(sessionRegistryPath))
            {
                UserName = usernameKey?.GetValue("username")?.ToString();
            }
            
            using (var sessionKey = Registry.CurrentUser.OpenSubKey(sessionRegistryPath))
            {
                scrobbler.SessionKey = sessionKey?.GetValue("sessionKey")?.ToString();
            }
        }
    }
}